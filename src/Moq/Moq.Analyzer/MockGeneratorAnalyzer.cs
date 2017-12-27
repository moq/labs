using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq.Properties;
using Stunts;

namespace Moq
{
    // TODO: F#
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MockGeneratorAnalyzer : StuntGeneratorAnalyzer
    {
        static readonly DiagnosticDescriptor missing = new DiagnosticDescriptor(
            "MOQ001",
            new ResourceString(nameof(Resources.MissingMockAnalyzer_Title)),
            new ResourceString(nameof(Resources.MissingMockAnalyzer_Message)),
            "Build",
            DiagnosticSeverity.Error,
            true,
            new ResourceString(nameof(Resources.MissingMockAnalyzer_Description)));

        static readonly DiagnosticDescriptor outdated = new DiagnosticDescriptor(
            "MOQ002",
            new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Title)),
            new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Message)),
            "Build",
            DiagnosticSeverity.Error,
            true,
            new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Description)));

        public override DiagnosticDescriptor MissingDescriptor => missing;

        public override DiagnosticDescriptor OutdatedDescriptor => outdated;

        public MockGeneratorAnalyzer() : base(new MockNamingConvention(), typeof(MockGeneratorAttribute)) { }
    }
}
