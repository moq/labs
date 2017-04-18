using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace Moq.Proxy
{
    public class AcceptanceTests : IClassFixture<AcceptanceTestsContext>
    {
        public const int AsyncTimeoutMilliseconds = 5000;

        ITestOutputHelper output;
        AcceptanceTestsContext context;

        public AcceptanceTests(ITestOutputHelper output, AcceptanceTestsContext context)
        {
            this.context = context;
            this.output = output;
        }

        [Trait("LongRunning", "true")]
        [MemberData(nameof(GetTypesToMock))]
        [Theory]
        public async Task CanGenerateAllProxies(string language, Type type, int index)
        {
            var project = context.GetProject(language);
            var compilation = await context.GetCompilationAsync(language, new CancellationTokenSource(AsyncTimeoutMilliseconds).Token);

            if (compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                Assert.False(true,
                    Environment.NewLine +
                    string.Join(Environment.NewLine, compilation
                        .GetDiagnostics()
                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                        .Select(d => d.ToString())));
            }

            var symbol = compilation.GetTypeByMetadataName(type.FullName);

            if (symbol != null)
            {
                var document = await new ProxyGenerator().GenerateProxyAsync(
                    context.Workspace, 
                    project,
                    new CancellationTokenSource(AsyncTimeoutMilliseconds).Token,
                    symbol);

                var syntax = await document.GetSyntaxRootAsync(new CancellationTokenSource(AsyncTimeoutMilliseconds).Token);
                document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax);

                await AssertCode.NoErrorsAsync(document);
            }
        }

        static IEnumerable<object[]> GetTypesToMock() => ReferencePaths.Paths
            // Some CodeAnalysis types have dependencies on editor/langsvc assemblies
            .Where(path => !path.EndsWith("mscorlib.dll") && !Path.GetFileName(path).StartsWith("Microsoft.CodeAnalysis"))
            .Select(path => Assembly.ReflectionOnlyLoadFrom(path))
            .SelectMany(asm => TryGetExportedTypes(asm)
            .Where(x => x.IsInterface && !x.IsGenericTypeDefinition && !typeof(Delegate).IsAssignableFrom(x)
                    // Hard-coded exclusions we know don't work
                    && !x.GetCustomAttributesData().Any(d => d.AttributeType == typeof(ObsoleteAttribute)) // Obsolete types could generate build errors
                    && x.Name[0] != '_'  // These are sort of internal...
                    && x.FullName != typeof(IProxy).FullName
                )
            )
#if QUICK
            .Take(1)
#endif
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
}
