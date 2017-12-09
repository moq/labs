using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Stunts.Properties;

namespace Stunts.Processors
{
    public class EnsureStuntsReference : IDocumentProcessor
    {
        static readonly string StuntsAssembly = Path.GetFileName(typeof(IStunt).Assembly.ManifestModule.FullyQualifiedName);

        public string Language => LanguageNames.CSharp;

        public ProcessorPhase Phase => ProcessorPhase.Prepare;

        public Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!document.Project.MetadataReferences.Any(r => r.Display.EndsWith(StuntsAssembly, StringComparison.Ordinal)))
                throw new ArgumentException(Strings.StuntsRequired(document.Project.Name));

            return Task.FromResult(document);
        }
    }
}
