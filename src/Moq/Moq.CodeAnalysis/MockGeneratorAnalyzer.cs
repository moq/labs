using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Analyzer that reports missing and outdated mocks.
    /// </summary>
    // TODO: F#
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MockGeneratorAnalyzer : StuntGeneratorAnalyzer
    {
        /// <inheritdoc />
        public override DiagnosticDescriptor MissingDiagnostic => MockDiagnostics.Missing;

        /// <inheritdoc />
        public override DiagnosticDescriptor OutdatedDiagnostic => MockDiagnostics.Outdated;

        /// <summary>
        /// Initializes the analyzer, passing the default <see cref="MockNamingConvention"/> 
        /// and <see cref="MockGeneratorAttribute"/> to the base class.
        /// </summary>
        public MockGeneratorAnalyzer() : base(new MockNamingConvention(), typeof(MockGeneratorAttribute), false) { }
    }
}
