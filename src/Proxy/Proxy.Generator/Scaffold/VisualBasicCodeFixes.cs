using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, GeneratorLayer.Scaffold)]
    [Shared]

    class VisualBasicCodeFixes : CodeFixDocumentVisitor
    {
        [ImportingConstructor]
        public VisualBasicCodeFixes(ICodeAnalysisServices services)
            : base(services, 
                  CodeFixNames.VisualBasic.ImplementAbstractClass, 
                  CodeFixNames.VisualBasic.ImplementInterface, 
                  CodeFixNames.VisualBasic.AddOverloads)
        {
        }
    }
}
