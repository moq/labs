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
        static readonly string StuntsAssembly = typeof(IStunt).Assembly.GetName().Name;
        static readonly string StuntsFile = Path.GetFileName(typeof(IStunt).Assembly.ManifestModule.FullyQualifiedName);
        
        public string Language => LanguageNames.CSharp;

        public ProcessorPhase Phase => ProcessorPhase.Prepare;

        public Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Stunts must either be a direct metadata/assembly reference
            if (!document.Project.MetadataReferences.Any(r => r.Display.EndsWith(StuntsFile, StringComparison.Ordinal)) && 
                // or a resolved project reference to the Stunts project (if someone is using it as source/submodule
                !document.Project.ProjectReferences.Select(r => document.Project.Solution.GetProject(r.ProjectId)?.AssemblyName)
                    .Any(n => StuntsAssembly.Equals(n, StringComparison.Ordinal)))
            {
                throw new ArgumentException(Strings.StuntsRequired(document.Project.Name));
            }

            return Task.FromResult(document);
        }
    }
}
