using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, GeneratorLayer.Scaffold)]
    [Shared]
    class VisualBasicImplementAbstractClass : ImplementAbstractClass
    {
        [ImportingConstructor]
        public VisualBasicImplementAbstractClass(ICodeAnalysisServices services)
            : base(services, LanguageNames.VisualBasic)
        {
        }
    }
}