﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Moq.Properties;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Generates code for mocks.
    /// </summary>
    // TODO: F#
    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(GenerateMockCodeFix))]
    public class GenerateMockCodeFix : StuntCodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MOQ001");

        /// <inheritdoc />
        protected override CodeAction CreateCodeAction(Document document, Diagnostic diagnostic)
            => new MockCodeAction(Strings.GenerateMockCodeFix.TitleFormat(
                diagnostic.Properties["TargetFullName"]), document, diagnostic);
    }

    /// <summary>
    /// Updates code for existing mocks.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(UpdateMockCodeFix))]
    public class UpdateMockCodeFix : StuntCodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("MOQ002");

        /// <inheritdoc />
        protected override CodeAction CreateCodeAction(Document document, Diagnostic diagnostic)
            => new MockCodeAction(Strings.UpdateMockCodeFix.TitleFormat(
                diagnostic.Properties["TargetFullName"]), document, diagnostic);
    }
}