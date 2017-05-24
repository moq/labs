using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Host;

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
                // If we request and process ALL codefixes at once, we'll get one for each 
                // diagnostics, which is one per non-implemented member of the interface/abstract 
                // base class, so we'd be applying unnecessary fixes after the first one.
                // So we re-retrieve them after each Apply, which will leave only the remaining 
                // ones.
                var codeFixes = await codeFixService.GetCodeFixes(document, codeFixName, cancellationToken);
                while (codeFixes.Length != 0)
                {
                    // We first try to apply all codefixes that don't involve our IProxy interface.
                    var codeFix = codeFixes.FirstOrDefault(x
                        => !x.Diagnostics.Any(d
                            => d.GetMessage().Contains(nameof(IProxy))));

                    if (codeFix == null)
                    {
                        // We have at least one codeFix for IProxy, pick last instance, which would be 
                        // the explicit implementation one.
                        codeFix = codeFixes.Last();
                    }

                    await codeFix.ApplyAsync(workspace, cancellationToken);
                    // Retrieve the updated document for the next pass.
                    document = workspace.CurrentSolution.GetDocument(document.Id);
                    // Retrieve the codefixes for the updated doc again.
                    codeFixes = await codeFixService.GetCodeFixes(document, codeFixName, cancellationToken);
                }
            }

            return document;
        }
    }
}
