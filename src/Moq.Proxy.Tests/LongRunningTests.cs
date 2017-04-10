using System;
using System.Collections.Generic;
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
            output.WriteLine($"{language}: {type.FullName}");

            var project = context.GetProject(language);
            var compilation = await context.GetCompilationAsync(language, new CancellationTokenSource(AsyncTimeoutMilliseconds).Token).ConfigureAwait(false);
            var document = await new ProxyGenerator().GenerateProxyAsync(context.Workspace, project,
                new CancellationTokenSource(AsyncTimeoutMilliseconds).Token,
                compilation.GetTypeByMetadataName(type.FullName))
                .ConfigureAwait(false);

            var syntax = await document.GetSyntaxRootAsync(new CancellationTokenSource(AsyncTimeoutMilliseconds).Token).ConfigureAwait(false);
            document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax);

            await AssertCode.NoErrorsAsync(document);
        }

        static IEnumerable<object[]> GetTypesToMock() =>
            Assembly.GetExecutingAssembly().GetExportedTypes()
                .Concat(Assembly.GetExecutingAssembly()
                    .GetReferencedAssemblies()
                    .Select(x => Assembly.Load(x))
                    .SelectMany(x => x.GetExportedTypes()))
                .Where(x => x.IsInterface && !x.IsGenericTypeDefinition && !typeof(Delegate).IsAssignableFrom(x) 
                    // Hard-coded exclusions we know don't work
                    //&& x != typeof(ICustomFormatter) // results in BC30530 because Format(string format...) doesn't work in VB
                    && x.Name[0] != '_'  // These are sort of internal...
                    && !x.FullName.StartsWith("Microsoft.CodeAnalysis")
                    && !x.FullName.StartsWith("Xunit")
                    && x.FullName != typeof(IProxy).FullName
                    // See ProxyGeneratorTests.CanGenerateIConfigurationSectionHandler
                    && x.FullName != typeof(System.Configuration.IConfigurationSectionHandler).FullName
                )
                .SelectMany((x, i) => new object[][]
                {
                    new object[] { LanguageNames.CSharp, x, i },
                    new object[] { LanguageNames.VisualBasic, x, i },
                })
#if DEBUG
                //.Skip(300).Take(100)
                //.Take(50)
#endif
                ;
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

            var references = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .Select(name => Assembly.Load(name))
                .Concat(new[] 
                {
                    typeof(IProxy).Assembly,
                    typeof(Task).Assembly,
                })
                .Concat(typeof(IProxy).Assembly.GetReferencedAssemblies().Select(x => Assembly.Load(x)))
                .Where(asm => File.Exists(asm.ManifestModule.FullyQualifiedName))
                .Select(asm => MetadataReference.CreateFromFile(asm.ManifestModule.FullyQualifiedName))
                .Concat(new[] { MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName) });

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
