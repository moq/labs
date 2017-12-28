using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq;

namespace Stunts
{
    /// <summary>
    /// Analyzes source code looking for method invocations to methods annotated with 
    /// the <see cref="StuntGeneratorAttribute"/> and reports unsupported types for 
    /// codegen.
    /// </summary>
    // TODO: F#
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MockUnsupportedNestedTypeAnalyzer : UnsupportedNestedTypeAnalyzer
    {
        /// <summary>
        /// Instantiates the analyzer with the default <see cref="NamingConvention"/> and 
        /// for method invocations annotated with <see cref="StuntGeneratorAttribute"/>.
        /// </summary>
        public MockUnsupportedNestedTypeAnalyzer() : base(new MockNamingConvention(), typeof(MockGeneratorAttribute)) { }
    }
}
