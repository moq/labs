using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Host;
using System.IO;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using Stunts.Properties;

namespace Stunts
{
    // NOTE: all behavior is implemented in the StuntCodeFix and the derived 
    // exported classes just provide the custom title depending on the 
    // analyzer that detected either a missing or outdated proxy.
    // TODO: F#
    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(GenerateStuntCodeFix)), Shared]
    public class GenerateStuntCodeFix : StuntCodeFixProvider
    {
        public GenerateStuntCodeFix()
            : base(Strings.GenerateStuntCodeFix.Title) { }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get => ImmutableArray.Create(MissingStuntAnalyzer.DiagnosticId);
        }

        //public sealed override FixAllProvider GetFixAllProvider() => new StuntFixAllProvider(Resources.GenerateStuntCodeFix_FixAllTitle, MissingStuntAnalyzer.DiagnosticId);
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(UpdateStuntCodeFix))]
    public class UpdateStuntCodeFix : StuntCodeFixProvider
    {
        public UpdateStuntCodeFix()
            : base(Strings.UpdateStuntCodeFix.Title) { }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get => ImmutableArray.Create(OutdatedStuntAnalyzer.DiagnosticId);
        }

        //public sealed override FixAllProvider GetFixAllProvider() => new StuntFixAllProvider(Resources.UpdateStuntCodeFix_FixAllTitle, OutdatedStuntAnalyzer.DiagnosticId);
    }

    public abstract class StuntCodeFixProvider : CodeFixProvider
    {
        string title;

        protected StuntCodeFixProvider(string title) => this.title = title;

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
                (SyntaxNode)token.Parent.AncestorsAndSelf().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.InvocationExpressionSyntax>().FirstOrDefault();

            if (invocation == null)
                return;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                new StuntCodeAction(title, context.Document, diagnostic, invocation),
                diagnostic);
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}