using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, DocumentVisitorLayer.Scaffold)]
    [Shared]

    class VisualBasicCodeFixes : IDocumentVisitor
    {
        static readonly string[] codeFixNames = new[]
        {
            CodeFixNames.VisualBasic.ImplementAbstractClass,
            CodeFixNames.VisualBasic.ImplementInterface,
            "OverrideAllMembersCodeFix",
            CodeFixNames.VisualBasic.AddOverloads,
        };

        ICodeFixService codeFixService;

        [ImportingConstructor]
        public VisualBasicCodeFixes(ICodeAnalysisServices services)
            => codeFixService = services.GetWorkspaceService<ICodeFixService>();

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var codeFixName in codeFixNames)
            {
                document = await codeFixService.ApplyAsync(codeFixName, document, cancellationToken);
            }

            return document;
        }
    }
}
