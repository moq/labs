using Microsoft.CodeAnalysis;
using Stunts;

namespace Moq
{
    class MockCodeAction : StuntCodeAction
    {
        public MockCodeAction(string title, Document document, Diagnostic diagnostic, SyntaxNode invocation) 
            : base(title, document, diagnostic, invocation, new MockNamingConvention())
        {
        }

        protected override StuntGenerator CreateGenerator(NamingConvention naming) => new MockGenerator(naming);
    }
}