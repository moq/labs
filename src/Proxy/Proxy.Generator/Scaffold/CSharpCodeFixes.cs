using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.CSharp, GeneratorLayer.Scaffold)]
    [Shared]

    class CSharpCodeFixes : CodeFixDocumentVisitor
    {
        [ImportingConstructor]
        public CSharpCodeFixes(ICodeAnalysisServices services) 
            : base(services, 
                  CodeFixNames.CSharp.ImplementAbstractClass, 
                  CodeFixNames.CSharp.ImplementInterface)
        {
        }
    }
}
