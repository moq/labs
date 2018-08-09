using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stunts;

namespace Moq
{
    // TODO: F#
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MockGeneratorAnalyzer : StuntGeneratorAnalyzer
    {
        public override DiagnosticDescriptor MissingDiagnostic => MockDiagnostics.Missing;

        public override DiagnosticDescriptor OutdatedDiagnostic => MockDiagnostics.Outdated;

        public MockGeneratorAnalyzer() : base(new MockNamingConvention(), typeof(MockGeneratorAttribute), false) { }
    }
}
