using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using CSFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using VBFactory = Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;
using Microsoft.CodeAnalysis.Simplification;
using System;

namespace Moq
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = "CustomDelegate")]
    public class CustomDelegateCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            "CS1503", // cannot convert from 'method group' to 'Action<...>'
            "BC31143" // Method '..' does not have a signature compatible with delegate '...'.
            );

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var span = context.Span;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var token = root.FindToken(span.Start);
            if (!token.Span.IntersectsWith(span))
                return;

            var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
            if (diagnostic == null)
                return;

            var semantic = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var node = root.FindNode(span);
            if (node == null)
                return;

            if (node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.Argument))
                node = ((CS.ArgumentSyntax)node).Expression;

            var setup = document.Project.Language == LanguageNames.CSharp ?
                (SyntaxNode)node.Ancestors().OfType<CS.InvocationExpressionSyntax>().FirstOrDefault() :
                node.Ancestors().OfType<VB.InvocationExpressionSyntax>().FirstOrDefault();

            if (setup == null)
                return;

            var setupSymbol = semantic.GetSymbolInfo(setup);
            if (setupSymbol.CandidateSymbols.IsDefaultOrEmpty)
                return;

            var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
            var scope = compilation.GetTypeByMetadataName(typeof(SetupScopeAttribute).FullName);
            if (scope == null)
                return;

            // We only generate for [SetupScope] annotated methods. 
            // TODO: should integrate seamlessly with recursive mocks
            if (!setupSymbol.CandidateSymbols.Any(c => c.GetAttributes().Any(a => a.AttributeClass == scope)))
                return;

            var memberSymbol = semantic.GetSymbolInfo(node);
            if (memberSymbol.CandidateSymbols.IsDefaultOrEmpty ||
                memberSymbol.CandidateSymbols.First().Kind != SymbolKind.Method)
                return;

            context.RegisterCodeFix(new SetupDelegateCodeAction(document, setup, (IMethodSymbol)memberSymbol.CandidateSymbols.First()), context.Diagnostics);
        }

        public sealed override FixAllProvider GetFixAllProvider() => null;

        public class SetupDelegateCodeAction : CodeAction
        {
            readonly Document document;
            readonly IMethodSymbol symbol;
            SyntaxNode setup;

            public SetupDelegateCodeAction(Document document, SyntaxNode setup, IMethodSymbol symbol)
            {
                this.document = document;
                this.setup = setup;
                this.symbol = symbol;
            }

            public override string Title => $"Generate delegate for {symbol.Name}";

            protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var generator = SyntaxGenerator.GetGenerator(document);
                var root = await setup.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
                var node = FindSetup(root);
                var member = setup.Ancestors().First(x => generator.GetDeclarationKind(x) == DeclarationKind.Method);
                var @class = member.Ancestors().First(x => generator.GetDeclarationKind(x) == DeclarationKind.Class);
                var @delegate = generator.GetMembers(@class)
                    // Just check by name for now. Might need to also ensure signature compatibility.
                    .FirstOrDefault(x => generator.GetName(x) == symbol.Name);

                var delegateName = symbol.Name;
                var signature = generator.DelegateDeclaration(
                        delegateName,
                        parameters: symbol.Parameters.Select(prm => generator.ParameterDeclaration(prm)),
                        returnType: symbol.ReturnsVoid ? null : generator.TypeExpression(symbol.ReturnType));

                if (@delegate == null)
                {
                    root = root.InsertNodesAfter(member, new[] { signature });
                    // Find the updated setup
                    node = FindSetup(root);
                }
                else
                {                    
                    var tempDoc = document.WithSyntaxRoot(generator.ReplaceNode(root, @delegate, signature));
                    tempDoc = await Simplifier.ReduceAsync(tempDoc).ConfigureAwait(false);
                    var tempRoot = await tempDoc.GetSyntaxRootAsync(cancellationToken);
                    var className = generator.GetName(@class);
                    var tempClass = tempRoot.DescendantNodes().First(x =>
                        generator.GetDeclarationKind(x) == DeclarationKind.Class &&
                        generator.GetName(x) == className);
                    var tempDelegate = generator.GetMembers(tempClass)
                        .First(x => generator.GetName(x) == delegateName);

                    if (!@delegate.IsEquivalentTo(tempDelegate, true))
                    {
                        // Generate the delegate name using full Type+Member name.
                        var semantic = await document.GetSemanticModelAsync(cancellationToken);
                        var mock = semantic.GetSymbolInfo(
                            setup.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression) ?
                            (SyntaxNode)((setup as CS.InvocationExpressionSyntax)?.Expression as CS.MemberAccessExpressionSyntax)?.Expression :
                            ((setup as VB.InvocationExpressionSyntax)?.Expression as VB.MemberAccessExpressionSyntax)?.Expression);

                        if (mock.Symbol != null && 
                            mock.Symbol.Kind == SymbolKind.Local || 
                            mock.Symbol.Kind == SymbolKind.Field)
                        {
                            var type = mock.Symbol.Kind == SymbolKind.Local ?
                                ((ILocalSymbol)mock.Symbol).Type :
                                ((IFieldSymbol)mock.Symbol).Type;

                            delegateName = type.MetadataName + symbol.Name;
                            signature = generator.WithName(signature, delegateName);
                            root = root.InsertNodesAfter(member, new[] { signature });
                            // Find the updated setup
                            node = FindSetup(root);
                        }
                    }
                }

                root = generator.ReplaceNode(root, node, generator.WithTypeArguments(node, generator.IdentifierName(delegateName)));
                // Find the updated setup
                node = FindSetup(root);

                // If there is no Returns, generate one
                if (node.Parent.Parent.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ExpressionStatement))
                {
                    var returns = generator.InvocationExpression(
                        generator.MemberAccessExpression(
                            node.Parent.WithTrailingTrivia(
                                node.Parent.Parent.GetLeadingTrivia().Add(CSFactory.Whitespace("\t"))),
                            "Returns"),
                        generator.ValueReturningLambdaExpression(
                            symbol.Parameters.Select(prm => generator.ParameterDeclaration(prm)),
                            generator.ThrowExpression(generator.NullLiteralExpression())));

                    // Replace the parent InvocationExpression with the returning one.
                    root = generator.ReplaceNode(root, node.Parent, returns);
                }
                else if (node.Parent.Parent.IsKind(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.ExpressionStatement))
                {
                    var lambda = VBFactory.MultiLineFunctionLambdaExpression(
                        VBFactory.FunctionLambdaHeader().WithParameterList(
                            VBFactory.ParameterList(
                                VBFactory.SeparatedList(
                                    symbol.Parameters.Select(prm => (VB.ParameterSyntax)generator.ParameterDeclaration(prm))))),
                        VBFactory.List(new VB.StatementSyntax[] 
                        {
                            VBFactory.ThrowStatement(
                                VBFactory.ObjectCreationExpression(
                                    VBFactory.ParseTypeName(nameof(NotImplementedException))))
                        }),
                        VBFactory.EndFunctionStatement());

                    var returns = generator.InvocationExpression(
                        generator.MemberAccessExpression(
                            node.Parent.WithTrailingTrivia(
                                node.Parent.Parent.GetLeadingTrivia()
                                    .Insert(0, VBFactory.Whitespace(" "))
                                    .Insert(1, VBFactory.LineContinuationTrivia(""))
                                    .Add(VBFactory.Whitespace("\t"))),
                            "Returns"),
                        lambda);

                    // Replace the parent InvocationExpression with the returning one.
                    root = generator.ReplaceNode(root, node.Parent, returns);
                }

                return document.WithSyntaxRoot(root);
            }

            SyntaxNode FindSetup(SyntaxNode root)
            {
                var node = root.FindNode(setup.Span, getInnermostNodeForTie: true);

                if (node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression))
                    node = ((CS.InvocationExpressionSyntax)node).Expression;
                else if (node.IsKind(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.InvocationExpression))
                    node = ((VB.InvocationExpressionSyntax)node).Expression;
                return node;
            }
        }
    }
}
