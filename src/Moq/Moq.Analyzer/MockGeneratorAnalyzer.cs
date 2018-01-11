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
        public override DiagnosticDescriptor MissingDescriptor => MockDiagnostics.Missing;

        public override DiagnosticDescriptor OutdatedDescriptor => MockDiagnostics.Outdated;

        public MockGeneratorAnalyzer() : base(new MockNamingConvention(), typeof(MockGeneratorAttribute), false) { }
    }
}
