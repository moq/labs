using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, GeneratorLayer.Scaffold)]
    [Shared]
    class VisualBasicImplementInterface : ImplementInterface
    {
        public VisualBasicImplementInterface()
            : base(LanguageNames.VisualBasic)
        {
        }
    }
}