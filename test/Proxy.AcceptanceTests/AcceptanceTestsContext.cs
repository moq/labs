using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.Threading;
using Xunit;

namespace Moq.Proxy
{
    public class AcceptanceTestsContext : IDisposable
    {
        AdhocWorkspace workspace;
        Project csproj;
        Project vbproj;
        AsyncLazy<Compilation> csbuild;
        AsyncLazy<Compilation> vbbuild;

        static AcceptanceTestsContext()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) =>
            {
                var name = new AssemblyName(args.Name);
                var file = name.Name + ".dll";
                var path = ReferencePaths.Paths.FirstOrDefault(x => x.EndsWith(file));
                if (path != null)
                    return Assembly.ReflectionOnlyLoadFrom(path);

                Assert.False(true, $"Failed to resolve {args.Name}.");
                return null;
            };
        }

        public AcceptanceTestsContext()
        {
            workspace = new AdhocWorkspace(ProxyGenerator.CreateHost());

            var references = ReferencePaths.Paths
                .Select(path => MetadataReference.CreateFromFile(path));

            csproj = workspace.AddProject("cscode", LanguageNames.CSharp)
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithMetadataReferences(references);

            csbuild = new AsyncLazy<Compilation>(() => csproj.GetCompilationAsync(new CancellationTokenSource(AcceptanceTests.AsyncTimeoutMilliseconds).Token));

            vbproj = workspace.AddProject("vbcode", LanguageNames.VisualBasic)
                .WithCompilationOptions(new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithMetadataReferences(references);

            vbbuild = new AsyncLazy<Compilation>(() => vbproj.GetCompilationAsync(new CancellationTokenSource(AcceptanceTests.AsyncTimeoutMilliseconds).Token));
        }

        public Workspace Workspace
            => workspace;

        public Project GetProject(string language)
            => language == LanguageNames.CSharp ? csproj : vbproj;

        public Task<Compilation> GetCompilationAsync(string language, CancellationToken cancellationToken)
            => language == LanguageNames.CSharp ? csbuild.GetValueAsync(cancellationToken) : vbbuild.GetValueAsync(cancellationToken);

        public void Dispose()
        {
            workspace.Dispose();
        }
    }
}
