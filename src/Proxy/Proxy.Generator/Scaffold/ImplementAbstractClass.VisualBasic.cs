using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, GeneratorLayer.Scaffold)]
    [Shared]
    class VisualBasicImplementAbstractClass : ImplementAbstractClass
    {
        public VisualBasicImplementAbstractClass()
            : base(LanguageNames.VisualBasic)
        {
        }
    }
}