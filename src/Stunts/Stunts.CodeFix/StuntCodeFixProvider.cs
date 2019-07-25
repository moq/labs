using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Stunts.Properties;

namespace Stunts
{
    /// <summary>
    /// Generates code for stunts.
    /// </summary>
    // NOTE: all behavior is implemented in the StuntCodeFixProvider and the derived 
    // exported classes just provide the custom title depending on the 
    // analyzer that detected either a missing or outdated proxy.
    // TODO: F#
    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(GenerateStuntCodeFix))]
    public class GenerateStuntCodeFix : StuntCodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("ST001");

        /// <summary>
        /// Creates the code action that will implement the fix.
        /// </summary>
        protected override CodeAction CreateCodeAction(Document document, Diagnostic diagnostic)
            => new StuntCodeAction(Strings.GenerateStuntCodeFix.TitleFormat(
                diagnostic.Properties["TargetFullName"]), document, diagnostic, new NamingConvention());
    }

    /// <summary>
    /// Updates code for existing stunts.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(UpdateStuntCodeFix))]
    public class UpdateStuntCodeFix : StuntCodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("ST002");

        /// <summary>
        /// Creates the code action that will implement the fix.
        /// </summary>
        protected override CodeAction CreateCodeAction(Document document, Diagnostic diagnostic)
            => new StuntCodeAction(Strings.UpdateStuntCodeFix.TitleFormat(
                diagnostic.Properties["TargetFullName"]), document, diagnostic, new NamingConvention());
    }

    /// <summary>
    /// Base class that implements the core code generation for stunts, which is reused 
    /// by the generation and updating.
    /// </summary>
    public abstract class StuntCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            // TODO: should we instead try to register for all diagnostics matching our fixable ones instead of just the first one?
            var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
            if (diagnostic == null)
                return;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CreateCodeAction(context.Document, diagnostic),
                diagnostic);
        }

        /// <summary>
        /// Creates the code action that will implement the fix.
        /// </summary>
        protected abstract CodeAction CreateCodeAction(Document document, Diagnostic diagnostic);

        /// <summary>
        /// Returns <see langword="null"/> since the generator does not support batch fixing.
        /// </summary>
        /// <returns></returns>
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        //public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
        public override FixAllProvider GetFixAllProvider()
            => new StuntFixAllProvider(FixableDiagnosticIds.First(), CreateCodeAction);
    }
}