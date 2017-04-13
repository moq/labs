using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Moq.Proxy.Tests
{
    public class LongRunningTests : IClassFixture<LongRunningContext>
    {
        public const int AsyncTimeoutMilliseconds = 5000;
        ITestOutputHelper output;
        LongRunningContext context;

        static LongRunningTests()
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
        
        public LongRunningTests(ITestOutputHelper output, LongRunningContext context)
        {
            this.context = context;
            this.output = output;
        }

        [Trait("LongRunning", "true")]
        [MemberData(nameof(GetTypesToMock))]
        [Theory]
        public async Task CanGenerateAllProxies(string language, Type type, int index)
        {
            //output.WriteLine($"{language}: {type.FullName}");

            var project = context.GetProject(language);
            var compilation = await context.GetCompilationAsync(language, new CancellationTokenSource(AsyncTimeoutMilliseconds).Token).ConfigureAwait(false);
            var symbol = compilation.GetTypeByMetadataName(type.FullName);

            if (symbol != null)
            {
                var document = await new ProxyGenerator().GenerateProxyAsync(
                    context.Workspace, 
                    project,
                    new CancellationTokenSource(AsyncTimeoutMilliseconds).Token,
                    symbol);

                var syntax = await document.GetSyntaxRootAsync(new CancellationTokenSource(AsyncTimeoutMilliseconds).Token).ConfigureAwait(false);
                document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax);

                await AssertCode.NoErrorsAsync(document);
            }
        }

        static IEnumerable<object[]> GetTypesToMock() => ReferencePaths.Paths
            // Some CodeAnalysis types have dependencies on editor/langsvc assemblies
            .Where(path => !path.EndsWith("mscorlib.dll") && !Path.GetFileName(path).StartsWith("Microsoft.CodeAnalysis"))
            .Select(path => Assembly.ReflectionOnlyLoadFrom(path))
            .Concat(new[] { Assembly.GetExecutingAssembly() })
            .SelectMany(asm => TryGetExportedTypes(asm)
            .Where(x => x.IsInterface && !x.IsGenericTypeDefinition && !typeof(Delegate).IsAssignableFrom(x)
                    // Hard-coded exclusions we know don't work
                    && !x.GetCustomAttributesData().Any(d => d.AttributeType == typeof(ObsoleteAttribute)) // Obsolete types could generate build errors
                    && x.Name[0] != '_'  // These are sort of internal...
                    && x.FullName != typeof(IProxy).FullName
                )
            )
            .SelectMany((x, i) => new object[][]
                {
                    new object[] { LanguageNames.CSharp, x, i },
                    new object[] { LanguageNames.VisualBasic, x, i },
                })
                ;

        static Type[] TryGetExportedTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch 
            {
                return new Type[0];
            }
        }
    }

    public class LongRunningContext : IDisposable
    {
        AdhocWorkspace workspace;
        Project csproj;
        Project vbproj;
        AsyncLazy<Compilation> csbuild;
        AsyncLazy<Compilation> vbbuild;

        public LongRunningContext()
        {
            workspace = new AdhocWorkspace(ProxyGenerator.CreateHost());

            var references = ReferencePaths.Paths
                .Select(path => MetadataReference.CreateFromFile(path));

            csproj = workspace.AddProject("cscode", LanguageNames.CSharp)
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithMetadataReferences(references);

            csbuild = new AsyncLazy<Compilation>(() => csproj.GetCompilationAsync(new CancellationTokenSource(LongRunningTests.AsyncTimeoutMilliseconds).Token));

            vbproj = workspace.AddProject("vbcode", LanguageNames.VisualBasic)
                .WithCompilationOptions(new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithMetadataReferences(references);

            vbbuild = new AsyncLazy<Compilation>(() => vbproj.GetCompilationAsync(new CancellationTokenSource(LongRunningTests.AsyncTimeoutMilliseconds).Token));
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
