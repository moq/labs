using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Moq.Properties;
using Moq.Sdk;
using Stunts;

namespace Moq.Processors
{
    class EnsureSdkReference : IDocumentProcessor
    {
        static readonly string MoqSdkAssembly = typeof(IMock).Assembly.GetName().Name;
        static readonly string MoqSdkFile = Path.GetFileName(typeof(IMock).Assembly.ManifestModule.FullyQualifiedName);

        public string Language => LanguageNames.CSharp;

        public ProcessorPhase Phase => ProcessorPhase.Prepare;

        public Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO: throwing doesn't seem useful at all here. 
            // Maybe we should have another analyzer that reports at a different level (compilation, semantic?).
            // But maybe the whole scenario is moot since the analyzer will come from Moq itself which will bring in the SDK, so...

            // Moq must either be a direct metadata/assembly reference
            if (!document.Project.MetadataReferences.Any(r => r.Display.EndsWith(MoqSdkFile, StringComparison.Ordinal)) &&
                // or a resolved project reference to the Stunts project (if someone is using it as source/submodule
                !document.Project.ProjectReferences.Select(r => document.Project.Solution.GetProject(r.ProjectId)?.AssemblyName)
                    .Any(n => MoqSdkAssembly.Equals(n, StringComparison.Ordinal)))
            {
                throw new ArgumentException(Strings.MoqRequired(document.Project.Name));
            }

            return Task.FromResult(document);
        }
    }
}
