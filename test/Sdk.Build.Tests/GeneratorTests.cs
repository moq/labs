using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Moq.Proxy;
using Moq.Sdk;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Moq.Tests
{
    public class GeneratorTests
    {
        ITestOutputHelper output;

        public GeneratorTests(ITestOutputHelper output) => this.output = output;

        [InlineData(LanguageNames.CSharp)]
        [Theory(Skip = "Manual Testing")]
        public async Task GeneratesAdditionalStuff(string languageName)
        {
            var workspace = new AdhocWorkspace(ProxyGenerator.CreateHost(typeof(MockGenerator).Assembly));
            var projectInfo = CreateProjectInfo(languageName, "code");
            var project = workspace.AddProject(projectInfo);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            var typeToProxy = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName);
            var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5),
                new ITypeSymbol[]
                {
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName),
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName),
                }.ToImmutableArray(), 
                new ITypeSymbol[]
                {
                    compilation.GetTypeByMetadataName(typeof(IMocked).FullName),
                }.ToImmutableArray());

            await AssertCode.NoErrorsAsync(document);

#if DEBUG
            output.WriteLine((await document.GetSyntaxRootAsync()).NormalizeWhitespace().ToString());
#endif

            //var assembly = compilation.Emit();
            //var proxyType = assembly.GetExportedTypes().FirstOrDefault();

            //Assert.NotNull(proxyType);
            //Assert.True(typeof(IDisposable).IsAssignableFrom(proxyType));
            //Assert.False(proxyType.FullName.Contains(nameof(IDisposable)),
            //    $"Generated proxy should not contain the additional type {nameof(IDisposable)} in its name.");
        }

    }
}
