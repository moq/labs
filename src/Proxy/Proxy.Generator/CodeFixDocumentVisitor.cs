using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Moq.Proxy
{
    abstract class CodeFixDocumentVisitor : IDocumentVisitor
    {
        ICodeFixService codeFixService;
        string[] codeFixNames;

        protected CodeFixDocumentVisitor(ICodeAnalysisServices services, params string[] codeFixes)
        {
            this.codeFixService = services.GetWorkspaceService<ICodeFixService>();
            this.codeFixNames = codeFixes;
        }

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var workspace = document.Project.Solution.Workspace;

            foreach (var codeFixName in codeFixNames)
            {
                ICodeFix codeFix;
                // If we request and process ALL codefixes at once, we'll get one for each 
                // diagnostics, which is one per non-implemented member of the interface/abstract 
                // base class, so we'd be applying unnecessary fixes after the first one.
                // So we re-retrieve them after each Apply, which will leave only the remaining 
                // ones.
                while ((codeFix = (await codeFixService.GetCodeFixes(document, codeFixName, cancellationToken)).FirstOrDefault()) != null)
                {
                    await codeFix.ApplyAsync(workspace, cancellationToken);
                    // Retrieve the updated document for the next pass.
                    document = workspace.CurrentSolution.GetDocument(document.Id);
                }
            }

            return document;
        }
    }
}
