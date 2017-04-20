using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Moq.Proxy.Tests
{
    public class ProxyGeneratorTests
    {
        ITestOutputHelper output;

        public ProxyGeneratorTests(ITestOutputHelper output) => this.output = output;

        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task INotifyPropertyChanged(string language)
            => CanGenerateProxy(language, typeof(INotifyPropertyChanged));

        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task ICustomFormatter(string language)
            => CanGenerateProxy(language, typeof(ICustomFormatter));
        
        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeHasGlobalNamespaceThenItWorks(string language)
            => CanGenerateProxy(language, typeof(IGlobal));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeIsAbstractTheItWorks(string language)
            => CanGenerateProxy(language, typeof(CalculatorBase));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeHasVirtualMembers(string language)
            => CanGenerateProxy(language, typeof(Calculator), trace: true);

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanGenerateProxyWithMultipleInterfaces(string language)
        {
            var (workspace, project) = CreateWorkspaceAndProject(language);

            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5),
                new[]
                {
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName),
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName),
                    compilation.GetTypeByMetadataName(typeof(ICalculator).FullName),
                    compilation.GetTypeByMetadataName(typeof(IFormattable).FullName),
                });

            var syntax = await document.GetSyntaxRootAsync();

            document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax);

            await AssertCode.NoErrorsAsync(document);
        }

        [Fact]
        public async Task WhenClassSymbolIsNotFirstThenThrows()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var types = new[]
            {
                compilation.GetTypeByMetadataName(typeof(ICalculator).FullName),
                compilation.GetTypeByMetadataName(typeof(Calculator).FullName),
            };

            await Assert.ThrowsAsync<ArgumentException>(() => new ProxyGenerator()
                .GenerateProxyAsync(workspace, project, TimeoutToken(5), types));
        }

        [Fact]
        public async Task WhenMultipleClassSymbolsThenThrows()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var types = new[]
            {
                compilation.GetTypeByMetadataName(typeof(object).FullName),
                compilation.GetTypeByMetadataName(typeof(Calculator).FullName),
            };

            await Assert.ThrowsAsync<ArgumentException>(() => new ProxyGenerator()
                .GenerateProxyAsync(workspace, project, TimeoutToken(5), types));
        }

        [Fact]
        public async Task WhenEnumSymbolIsSpecifiedThenThrows()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var types = new[]
            {
                compilation.GetTypeByMetadataName(typeof(PlatformID).FullName),
            };

            await Assert.ThrowsAsync<ArgumentException>(() => new ProxyGenerator()
                .GenerateProxyAsync(workspace, project, TimeoutToken(5), types));
        }

        async Task CanGenerateProxy(string language, Type type, bool trace = false)
        {
            var (workspace, project) = CreateWorkspaceAndProject(language);

            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5),
                new[]
                {
                    compilation.GetTypeByMetadataName(type.FullName),
                });

            var syntax = await document.GetSyntaxRootAsync();

            document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax);

            await AssertCode.NoErrorsAsync(document);

            if (trace)
                output.WriteLine(syntax.NormalizeWhitespace().ToFullString());
        }
    }
}

public interface IGlobal
{
    void Do();
}