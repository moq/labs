using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Stunts.Properties;
using CSharp = Microsoft.CodeAnalysis.CSharp.Syntax;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Stunts
{
    /// <summary>
    /// Allows automatically fixing the ordering of mock generator method 
    /// type parameters.
    /// </summary>
    // TODO: not working yet
    // [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MoveBaseTypeFirstCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(StuntDiagnostics.BaseTypeNotFirst.Id);

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var span = context.Span;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken);

            var token = root.FindToken(span.Start);
            if (!token.Span.IntersectsWith(span))
                return;

            // Getting the inner-most ensure we get the type identifiers, rather 
            // than the SimpleBaseTypeSyntax, from which we can't get the symbol.
            var node = root.FindNode(span, getInnermostNodeForTie: true);
            if (node == null)
                return;

            if (node.Language == LanguageNames.CSharp ||
                node.Language == LanguageNames.VisualBasic)
            {
                context.RegisterCodeFix(new MoveBaseTypeFirstCodeAction(document, node), context.Diagnostics);
            }
        }

        class MoveBaseTypeFirstCodeAction : CodeAction
        {
            readonly Document document;
            SyntaxNode node;

            public MoveBaseTypeFirstCodeAction(Document document, SyntaxNode node)
            {
                this.document = document;
                this.node = node;
            }

            public override string Title => Resources.MoveBaseTypeFirst_Title;

            protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var generator = SyntaxGenerator.GetGenerator(document);
                var semantic = await document.GetSemanticModelAsync(cancellationToken);

                (SyntaxNode syntax, INamedTypeSymbol symbol)[] typeArguments;

                if (node.Language == LanguageNames.CSharp)
                {
                    // In C#, the member access is actually the Expression inside an 
                    // invocation syntax
                    node = ((CSharp.InvocationExpressionSyntax)node).Expression;
                    typeArguments = node
                        .DescendantNodes()
                        .OfType<CSharp.TypeArgumentListSyntax>()
                        .SelectMany(x => x.ChildNodes().OfType<CSharp.IdentifierNameSyntax>())
                        .Select(x => ((SyntaxNode)x, semantic.GetSymbolInfo(x, cancellationToken).Symbol as INamedTypeSymbol))
                        .ToArray();
                }
                else
                {
                    typeArguments = node
                        .ChildNodes().OfType<VisualBasic.GenericNameSyntax>()
                        .SelectMany(g => g.ChildNodes().OfType<VisualBasic.TypeArgumentListSyntax>())
                        .SelectMany(g => g.ChildNodes().OfType<VisualBasic.IdentifierNameSyntax>())
                        .Select(x => ((SyntaxNode)x, semantic.GetSymbolInfo(x, cancellationToken).Symbol as INamedTypeSymbol))
                        .ToArray();
                }

                // Hardly optimal, but this codefix should run rarely and only 
                // for the few type arguments a typical stunt would implement.
                var updated = generator.WithTypeArguments(node,
                    new[] { typeArguments.First(x => x.symbol?.TypeKind == TypeKind.Class) }
                    .Concat(typeArguments.Where(x => x.symbol?.TypeKind != TypeKind.Class))
                    .Select(x => x.syntax));

                return document.WithSyntaxRoot(await updated.SyntaxTree.GetRootAsync(cancellationToken));
            }
        }
    }
}
