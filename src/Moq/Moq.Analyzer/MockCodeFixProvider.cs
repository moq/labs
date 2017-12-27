using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Moq.Properties;
using Stunts;

namespace Moq
{
    // TODO: F#
    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(GenerateMockCodeFix))]
    public class GenerateMockCodeFix : StuntCodeFixProvider
    {
        public GenerateMockCodeFix()
            : base(Strings.GenerateMockCodeFix.Title) { }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MOQ001");

        protected override CodeAction CreateCodeAction(Document document, Diagnostic diagnostic, SyntaxNode invocation)
            => new MockCodeAction(Title, document, diagnostic, invocation);
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(UpdateMockCodeFix))]
    public class UpdateMockCodeFix : StuntCodeFixProvider
    {
        public UpdateMockCodeFix()
            : base(Strings.UpdateMockCodeFix.Title) { }

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MOQ002");

        protected override CodeAction CreateCodeAction(Document document, Diagnostic diagnostic, SyntaxNode invocation)
            => new MockCodeAction(Title, document, diagnostic, invocation);
    }
}