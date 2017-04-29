using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.CSharp, GeneratorLayer.Scaffold)]
    [Shared]
    class CSharpImplementInterface : ImplementInterface
    {
        [ImportingConstructor]
        public CSharpImplementInterface(ICodeAnalysisServices services)
            : base(services, LanguageNames.CSharp)
        {
        }
    }
}