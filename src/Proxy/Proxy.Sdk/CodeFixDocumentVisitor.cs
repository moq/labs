using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Host;

namespace Moq.Proxy
{
    public abstract class CodeFixDocumentVisitor : IDocumentVisitor
    {
        ICodeFixService codeFixService;
        string[] codeFixNames;

        protected CodeFixDocumentVisitor(ICodeAnalysisServices services, params string[] codeFixes)
        {
            codeFixService = services.GetWorkspaceService<ICodeFixService>();
            codeFixNames = codeFixes;
        }

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
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
                    var operations = await codeFixes[0].Action.GetOperationsAsync(cancellationToken);
                    ApplyChangesOperation operation;
                    if ((operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault()) != null)
                    {
                        document = operation.ChangedSolution.GetDocument(document.Id);
                        // Retrieve the codefixes for the updated doc again.
                        codeFixes = await codeFixService.GetCodeFixes(document, codeFixName, cancellationToken);
                    }
                    else
                    {
                        // If we got no applicable code fixes, exit the loop and move on to the next codefix.
                        break;
                    }
                }
            }

            return document;
        }
    }
}
