using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stunts;

namespace Moq
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MockValidateTypesAnalyzer : ValidateTypesAnalyzer
    {
        public MockValidateTypesAnalyzer()
            : base(typeof(MockGeneratorAttribute)) { }
    }
}
