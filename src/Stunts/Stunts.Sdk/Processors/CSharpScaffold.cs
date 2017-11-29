using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Stunts.Processors
{
    public class CSharpScaffold : IDocumentProcessor
    {
        static readonly string[] codeFixNames = 
        {
            CodeFixNames.CSharp.ImplementAbstractClass,
            CodeFixNames.CSharp.ImplementInterface,
            "OverrideAllMembersCodeFix",
        };

        SyntaxGenerator generator;
        CancellationToken cancellationToken;

        public string Language => LanguageNames.CSharp;

        public ProcessorPhase Phase => ProcessorPhase.Scaffold;

        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.cancellationToken = cancellationToken;
            generator = SyntaxGenerator.GetGenerator(document);

            foreach (var codeFixName in codeFixNames)
            {
                document = await document.ApplyCodeFixAsync(codeFixName, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }
    }
}
