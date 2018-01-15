using Microsoft.CodeAnalysis;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Overrides Stunts.Sdk's built-in <see cref="StuntCodeAction"/> 
    /// with our own custom generator and naming conventions.
    /// </summary>
    class MockCodeAction : StuntCodeAction
    {
        public MockCodeAction(string title, Document document, Diagnostic diagnostic) 
            : base(title, document, diagnostic, new MockNamingConvention())
        {
        }
        
        protected override StuntGenerator CreateGenerator(NamingConvention naming) => new MockGenerator(naming);
    }
}