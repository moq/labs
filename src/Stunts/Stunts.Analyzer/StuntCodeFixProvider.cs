using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Stunts.Properties;

namespace Stunts
{
    // NOTE: all behavior is implemented in the StuntCodeFixProvider and the derived 
    // exported classes just provide the custom title depending on the 
    // analyzer that detected either a missing or outdated proxy.
    // TODO: F#
    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(GenerateStuntCodeFix))]
    public class GenerateStuntCodeFix : StuntCodeFixProvider
    {
        public GenerateStuntCodeFix() : base(Strings.GenerateStuntCodeFix.Title) { }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("ST001");
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(UpdateStuntCodeFix))]
    public class UpdateStuntCodeFix : StuntCodeFixProvider
    {
        public UpdateStuntCodeFix() : base(Strings.UpdateStuntCodeFix.Title) { }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("ST002");
    }

    public abstract class StuntCodeFixProvider : CodeFixProvider
    {
        protected StuntCodeFixProvider(string title) => Title = title;

        public string Title { get; }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
            if (diagnostic == null)
                return;

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

            // Find the invocation identified by the diagnostic.
            var invocation =
                // TODO: F#
                (SyntaxNode)token.Parent.AncestorsAndSelf().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>().FirstOrDefault() ??
                (SyntaxNode)token.Parent.AncestorsAndSelf().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.MemberAccessExpressionSyntax>().FirstOrDefault();

            if (invocation == null)
                return;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CreateCodeAction(context.Document, diagnostic, invocation),
                diagnostic);
        }

        /// <summary>
        /// Creates the code action that will implement the fix.
        /// </summary>
        protected virtual CodeAction CreateCodeAction(Document document, Diagnostic diagnostic, SyntaxNode invocation)
            => new StuntCodeAction(Title, document, diagnostic, invocation, new NamingConvention());

        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    }
}