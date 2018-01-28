using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Moq.Processors;
using Moq.Properties;
using Moq.Sdk;
using Stunts;
using Stunts.Processors;

namespace Moq
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = "CustomMock")]
    [ExtensionOrder(Before = "ImplementInterface")]
    public class CustomMockCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            // See http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp.Features/ImplementAbstractClass/CSharpImplementAbstractClassCodeFixProvider.cs,15
            "CS0534",
            // See http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp.Features/ImplementInterface/CSharpImplementInterfaceCodeFixProvider.cs,24
            "CS0535",
            // See http://source.roslyn.io/#Microsoft.CodeAnalysis.VisualBasic.Features/ImplementAbstractClass/VisualBasicImplementAbstractClassCodeFixProvider.vb,15
            "BC30610",
            // See http://source.roslyn.io/#Microsoft.CodeAnalysis.VisualBasic.Features/ImplementInterface/VisualBasicImplementInterfaceCodeFixProvider.vb,16
            "BC30149");

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var span = context.Span;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var token = root.FindToken(span.Start);
            if (!token.Span.IntersectsWith(span))
                return;

            var model = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var generator = SyntaxGenerator.GetGenerator(document);

            // Getting the inner-most ensure we get the type identifiers, rather 
            // than the SimpleBaseTypeSyntax, from which we can't get the symbol.
            var node = root.FindNode(span, getInnermostNodeForTie: true);
            if (node == null)
                return;

            bool isMocked(SyntaxNode n)
            {
                var symbol = model.GetSymbolInfo(n, context.CancellationToken);
                return
                    symbol.Symbol != null &&
                    symbol.Symbol.ToString() == typeof(IMocked).FullName &&
                    symbol.Symbol.ContainingAssembly.Name == typeof(IMocked).Assembly.GetName().Name;
            }

            // If we find a symbol that happens to be IMocked, implement the core interface.
            if (generator.GetDeclarationKind(node) != DeclarationKind.Class && isMocked(node))
            {
                context.RegisterCodeFix(new ImplementMockCodeAction(context.Document), context.Diagnostics);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider() => null;

        class ImplementMockCodeAction : CodeAction
        {
            readonly Document document;

            public ImplementMockCodeAction(Document document) => this.document = document;

            public override string Title => Strings.CustomMockCodeFix.Implement;

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
                => new StuntGenerator(
                    new DefaultImports(typeof(LazyInitializer).Namespace, typeof(IMocked).Namespace),
                    new CSharpMocked(),
                    new CSharpCompilerGenerated(),
                    new VisualBasicMocked(),
                    new VisualBasicCompilerGenerated())
                .ApplyProcessors(document, cancellationToken);
        }
    }
}
