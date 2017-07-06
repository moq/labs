using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Moq.Proxy;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;
using MSBuild = Microsoft.Build.Utilities;

namespace Moq.Sdk.Tests
{
    public class GeneratorTests
    {
        ITestOutputHelper output;

        public GeneratorTests(ITestOutputHelper output) => this.output = output;

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanGenerateProxies(string languageName)
        {
#if DEBUG
            var config = "Debug";
#else
            var config = "Release";
#endif
            var task = new GenerateProxies
            {
                BuildEngine = new MockBuildEngine(output),
                LanguageName = languageName,
                ToolPath = new DirectoryInfo($@"..\..\..\..\src\Proxy\Proxy.Generator.Console\bin\{config}").FullName,
                OutputPath = new DirectoryInfo(".").FullName,
                References = ReferencePaths.Paths
                    .Concat(new[] { typeof(GeneratorTests).Assembly.ManifestModule.FullyQualifiedName })
                    .Select(x => new MSBuild.TaskItem(x)).ToArray(),
                AdditionalGenerators = new[] { new MSBuild.TaskItem(typeof(CSharpMocked).Assembly.ManifestModule.FullyQualifiedName) },
                AdditionalInterfaces = new[] { new MSBuild.TaskItem(typeof(IMocked).FullName) },
                AdditionalProxies = new[] { new MSBuild.TaskItem(typeof(ICalculator).FullName) },
            };

            Assert.True(task.Execute());
            Assert.Equal(1, task.Proxies.Count());

            var (workspace, project) = CreateWorkspaceAndProject(languageName);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var document = project.AddDocument("proxy." + (languageName == LanguageNames.CSharp ? "cs" : "vb"),
                File.ReadAllText(task.Proxies[0].GetMetadata("FullPath")), filePath: task.Proxies[0].GetMetadata("FullPath"));

            await AssertCode.NoErrorsAsync(document);
        }
    }
}
