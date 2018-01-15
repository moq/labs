using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Stunts.Processors;
using Stunts.Properties;

namespace Stunts
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = "CustomStunt")]
    [ExtensionOrder(Before = "ImplementInterface")]
    public class CustomStuntCodeFixProvider : CodeFixProvider
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

            bool isStunt(SyntaxNode n)
            {
                var symbol = model.GetSymbolInfo(n, context.CancellationToken);
                return
                    symbol.Symbol != null &&
                    symbol.Symbol.ToString() == typeof(IStunt).FullName &&
                    symbol.Symbol.ContainingAssembly.Name == typeof(IStunt).Assembly.GetName().Name;
            }

            bool isGenerated(SyntaxNode n)
            {
                var symbol = model.GetSymbolInfo(n, context.CancellationToken);
                // [CompilerGenerated] is used in IStunt and IMocked to signal that 
                // no implementation should be offered thought the behavior pipeline
                // because a custom code fix provides the implementation instead.
                return
                    symbol.Symbol != null &&
                    symbol.Symbol.GetAttributes().Any(attr =>
                        attr.AttributeClass.ToFullMetadataName() == typeof(GeneratedCodeAttribute).FullName ||
                        attr.AttributeClass.ToFullMetadataName() == typeof(CompilerGeneratedAttribute).FullName);
            }

            // If we find a symbol that happens to be IStunt, implement the core interface.
            if (generator.GetDeclarationKind(node) != DeclarationKind.Class && isStunt(node))
            {
                context.RegisterCodeFix(new ImplementStuntCodeAction(context.Document), context.Diagnostics);
            }
            else if (!isGenerated(node))
            {
                // Make sure the interface or abstract class is on a type that implements IStunt
                // NOTE: if the node is already the class declaration (such as for abstract base class)
                // this call returns the same declaration. Otherwise, it will return the declaration that 
                // an interface is added to.
                var declaration = generator.GetDeclaration(node);
                if (declaration != null && generator.GetBaseAndInterfaceTypes(declaration).Any(isStunt))
                    context.RegisterCodeFix(new ImplementThroughStuntCodeAction(context.Document), context.Diagnostics);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider() => null;

        class ImplementStuntCodeAction : CodeAction
        {
            readonly Document document;

            public ImplementStuntCodeAction(Document document) => this.document = document;

            public override string Title => Strings.CustomStuntCodeFix.ImplementStunt;

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
                => new StuntGenerator(
                    new DefaultImports(),
                    new CSharpFileHeader(),
                    new CSharpScaffold("OverrideAllMembersCodeFix"),
                    new CSharpRewrite(),
                    new CSharpStunt(),
                    new CSharpCompilerGenerated(),
                    new VisualBasicFileHeader(),
                    new VisualBasicScaffold("OverrideAllMembersCodeFix"),
                    new VisualBasicRewrite(),
                    new VisualBasicStunt(),
                    new VisualBasicCompilerGenerated())
                .ApplyProcessors(document, cancellationToken);
        }

        class ImplementThroughStuntCodeAction : CodeAction
        {
            readonly Document document;

            public ImplementThroughStuntCodeAction(Document document) => this.document = document;

            public override string Title => Strings.CustomStuntCodeFix.ImplementThroughBehavior;

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
                => new StuntGenerator(
                    new CSharpFileHeader(),
                    new CSharpScaffold(),
                    new CSharpRewrite(),
                    new CSharpCompilerGenerated(),
                    new VisualBasicFileHeader(),
                    new VisualBasicScaffold(),
                    new VisualBasicRewrite(),
                    new VisualBasicParameterFixup(),
                    new VisualBasicCompilerGenerated())
                .ApplyProcessors(document, cancellationToken);
        }
    }
}
