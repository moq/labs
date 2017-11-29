using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Stunts.Processors
{
    public class VisualBasicScaffold : IDocumentProcessor
    {
        static readonly string[] codeFixNames = 
        {
            CodeFixNames.VisualBasic.ImplementAbstractClass,
            CodeFixNames.VisualBasic.ImplementInterface,
            "OverrideAllMembersCodeFix",
            CodeFixNames.VisualBasic.AddOverloads,
        };

        public string Language => LanguageNames.VisualBasic;

        public ProcessorPhase Phase => ProcessorPhase.Scaffold;

        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var codeFixName in codeFixNames)
            {
                document = await document.ApplyCodeFixAsync(codeFixName, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }
    }
}
