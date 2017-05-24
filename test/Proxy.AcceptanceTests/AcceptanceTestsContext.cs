using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.Threading;
using static TestHelpers;

namespace Moq.Proxy
{
    public class AcceptanceTestsContext : IDisposable
    {
        static readonly AdhocWorkspace workspace = new AdhocWorkspace(ProxyGenerator.CreateHost());
        //The location of the .NET assemblies
        static readonly string frameworkPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        static readonly MetadataReference[] references = new[]
            {
                Path.Combine(frameworkPath, "mscorlib.dll"),
                Path.Combine(frameworkPath, "System.dll"),
                Path.Combine(frameworkPath, "System.Core.dll"),
                Path.Combine(frameworkPath, "System.Reflection.dll"),
                Path.Combine(frameworkPath, "System.Runtime.dll"),
            }
            .Concat(ReferencePaths.Paths)
            .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path))
            .Distinct(FileNameEqualityComparer.Default)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();

        Lazy<Project> csproj;
        Lazy<Project> vbproj;
        AsyncLazy<Compilation> csbuild;
        AsyncLazy<Compilation> vbbuild;

        public AcceptanceTestsContext()
        {
            csproj = new Lazy<Project>(() => workspace.AddProject(ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "cscode",
                "cscode.dll",
                LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: references)));

            csbuild = new AsyncLazy<Compilation>(() => csproj.Value.GetCompilationAsync(new CancellationTokenSource(AcceptanceTests.AsyncTimeoutMilliseconds).Token));

            vbproj = new Lazy<Project>(() => workspace.AddProject(ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "vbcode",
                "vbcode.dll",
                LanguageNames.VisualBasic,
                compilationOptions: new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionStrict: OptionStrict.On),
                metadataReferences: references)));

            vbbuild = new AsyncLazy<Compilation>(() => vbproj.Value.GetCompilationAsync(new CancellationTokenSource(AcceptanceTests.AsyncTimeoutMilliseconds).Token));
        }

        public AdhocWorkspace Workspace
            => workspace;

        public Project GetProject(string language)
            => language == LanguageNames.CSharp ? csproj.Value : vbproj.Value;

        public Task<Compilation> GetCompilationAsync(string language, CancellationToken cancellationToken)
            => language == LanguageNames.CSharp ? csbuild.GetValueAsync(cancellationToken) : vbbuild.GetValueAsync(cancellationToken);

        public void Dispose()
        {
            workspace.Dispose();
        }
    }
}
