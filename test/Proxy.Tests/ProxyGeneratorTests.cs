using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ManualProxies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Simplification;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Moq.Proxy.Tests
{
    public class ProxyGeneratorTests
    {
        ITestOutputHelper output;

        public ProxyGeneratorTests(ITestOutputHelper output) => this.output = output;

        //[InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task GeneratedInterfaceHasCompilerGeneratedAttribute(string languageName)
        {
            var compilation = await CanGenerateProxy(languageName, typeof(INotifyPropertyChanged));
            var assembly = compilation.Emit();
            var proxyType = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(proxyType);
            Assert.True(proxyType.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any(),
                "Generated proxy did not have the 'CompilerGeneratedAttribute' attribute applied.");
        }

        [Fact]
        public Task INotifyPropertyChanged()
            => CanGenerateProxy(LanguageNames.VisualBasic, typeof(INotifyPropertyChanged));

        [Fact]
        public Task ITypeGetter()
            => CanGenerateProxy(LanguageNames.VisualBasic, typeof(ITypeGetter));

        [Fact]
        public Task ICustomFormatter()
            => CanGenerateProxy(LanguageNames.VisualBasic, typeof(ICustomFormatter));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeHasGlobalNamespaceThenItWorks(string language)
            => CanGenerateProxy(language, typeof(IGlobal));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeIsInterface(string language)
            => CanGenerateProxy(language, typeof(ICalculator));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeIsAbstract(string language)
            => CanGenerateProxy(language, typeof(CalculatorBase));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeHasVirtualMembers(string language)
            => CanGenerateProxy(language, typeof(Calculator));

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
                });

            var syntax = await document.GetSyntaxRootAsync();

            document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax,
                filePath: Path.GetTempFileName());

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

        async Task<Compilation> CanGenerateProxy(string language, Type type, bool trace = false)
        {
            var (workspace, project) = CreateWorkspaceAndProject(language, type.FullName);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5),
                new[]
                {
                    compilation.GetTypeByMetadataName(type.FullName),
                });

            var syntax = await document.GetSyntaxRootAsync();

            document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax, filePath: Path.GetTempFileName());

            await AssertCode.NoErrorsAsync(document);

            if (trace)
            {
                document = await Simplifier.ReduceAsync(document);
                var root = await document.GetSyntaxRootAsync();
                output.WriteLine(root.NormalizeWhitespace().ToFullString());
            }

            return await document.Project.GetCompilationAsync();
        }
    }
}

public interface IGlobal
{
    void Do();
}