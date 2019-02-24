using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;

namespace Moq.Migration
{
    /// <summary>
    /// new Mock{T}() => Mock.Of{T}()
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    internal class MOQ1000NewMockT : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="MOQ1000NewMockT"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "MOQ1000";

        const string Title = "Migrate to Moq.Of";
        const string MessageFormat = "The Moq<T> type has been replaced by direct usage of Moq.Of<T>, which returns an instance of T.";
        const string Description = "All Mock<T> operations such as Setup and Returns have been replaced with corresponding extension methods on the T instance itself, so Moq.Of<T> should be used exclusively. Yes, this means mock.Object is gone. Yay :)";
        const string HelpLink = "https://github.com/mow/moq/blob/master/docs/MOQ1000.md";

        static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Moq.Migration", DiagnosticSeverity.Error, true, Description, HelpLink);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreate, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreate, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.ObjectCreationExpression);
        }

        void AnalyzeObjectCreate(SyntaxNodeAnalysisContext context)
        {
            var semantic = context.Compilation.GetSemanticModel(context.Node.SyntaxTree);
            var symbol = semantic.GetSymbolInfo(context.Node);
            if (symbol.Symbol == null)
                return;

            var mock = context.Compilation.GetTypeByMetadataName("Moq.Mock`1");
            if (mock == null)
                return;
            
            if (symbol.Symbol.MetadataName == WellKnownMemberNames.InstanceConstructorName && 
                symbol.Symbol is IMethodSymbol ctor &&
                ctor.ReceiverType is INamedTypeSymbol type &&
                type.IsGenericType &&
                type.ConstructedFrom == mock)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
        }
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(MOQ1000CodeFixProvider))]
    [Shared]
    internal class MOQ1000CodeFixProvider : CodeFixProvider
    {
        const string TitleCS = "Replace with Moq.Of<T>";
        const string TitleVB = "Replace with Moq.Of(T)";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(MOQ1000NewMockT.DiagnosticId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        context.Document.Project.Language == LanguageNames.CSharp ? TitleCS : TitleVB,
                        cancellation => CreateChangedSolutionAsync(context, diagnostic, cancellation),
                        nameof(MOQ1000CodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private async Task<Document> CreateChangedSolutionAsync(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellation)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

            IEnumerable<SyntaxNode> typeArguments;
            IEnumerable<SyntaxNode> arguments;

            if (context.Document.Project.Language == LanguageNames.CSharp)
            {
                var ctorSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax)token.Parent;
                typeArguments = ((Microsoft.CodeAnalysis.CSharp.Syntax.GenericNameSyntax)ctorSyntax.Type).TypeArgumentList.Arguments;
                arguments = ctorSyntax.ArgumentList?.Arguments ?? Enumerable.Empty<SyntaxNode>();
            }
            else
            {
                var ctorSyntax = (Microsoft.CodeAnalysis.VisualBasic.Syntax.ObjectCreationExpressionSyntax)token.Parent;
                typeArguments = ((Microsoft.CodeAnalysis.VisualBasic.Syntax.GenericNameSyntax)ctorSyntax.Type).TypeArgumentList.Arguments;
                arguments = ctorSyntax.ArgumentList?.Arguments ?? Enumerable.Empty<SyntaxNode>();
            }

            var syntax = root.FindNode(context.Span);
            var generator = SyntaxGenerator.GetGenerator(context.Document);

            var ofT = generator.InvocationExpression(
                generator.MemberAccessExpression(
                    generator.IdentifierName("Mock"),
                    generator.GenericName("Of", typeArguments)), 
                arguments);

            var newRoot = generator.ReplaceNode(root, syntax, ofT);

            return context.Document.WithSyntaxRoot(newRoot);
        }
    }
}