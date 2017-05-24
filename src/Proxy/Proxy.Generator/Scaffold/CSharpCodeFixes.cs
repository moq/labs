using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.CSharp, DocumentVisitorLayer.Scaffold)]
    [Shared]

    class CSharpCodeFixes : CodeFixDocumentVisitor
    {
        [ImportingConstructor]
        public CSharpCodeFixes(ICodeAnalysisServices services)
            : base(services,
                  CodeFixNames.CSharp.ImplementAbstractClass,
                  CodeFixNames.CSharp.ImplementInterface)
        // NOTE: should we also add CodeFixNames.All.RemoveUnnecessaryImports?
        {
        }
    }
}
