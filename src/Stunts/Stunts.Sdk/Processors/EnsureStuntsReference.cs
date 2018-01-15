using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Stunts.Properties;

namespace Stunts.Processors
{
    /// <summary>
    /// Verifies that the document's project has the required reference to the Stunts assembly.
    /// </summary>
    public class EnsureStuntsReference : IDocumentProcessor
    {
        static readonly string StuntsAssembly = typeof(IStunt).Assembly.GetName().Name;
        static readonly string StuntsFile = Path.GetFileName(typeof(IStunt).Assembly.ManifestModule.FullyQualifiedName);

        /// <summary>
        /// Applies to both <see cref="LanguageNames.CSharp"/> and <see cref="LanguageNames.VisualBasic"/>.
        /// </summary>
        public string[] Languages { get; } = new[] { LanguageNames.CSharp, LanguageNames.VisualBasic };

        /// <summary>
        /// Runs in the first phase of code gen, <see cref="ProcessorPhase.Prepare"/>.
        /// </summary>
        public ProcessorPhase Phase => ProcessorPhase.Prepare;

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the document's project does not 
        /// have a reference to the Stunts assembly.
        /// </summary>
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
