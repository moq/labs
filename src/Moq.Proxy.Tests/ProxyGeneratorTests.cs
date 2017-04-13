using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using static Moq.Proxy.Tests.TestHelpers;

namespace Moq.Proxy.Tests
{
    public class ProxyGeneratorTests
    {
        ITestOutputHelper output;

        public ProxyGeneratorTests(ITestOutputHelper output) => this.output = output;

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task IDbCommandInterceptor(string language)
            => CanGenerateProxy(language, typeof(System.Data.Entity.Infrastructure.Interception.IDbCommandInterceptor));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanGenerateProxy(string language)
        {
            var (workspace, project) = CreateWorkspaceAndProject(language);

            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5),
                new[]
                {
                    compilation.GetTypeByMetadataName(typeof(ISourceAssemblySymbol).FullName),
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName),
                    compilation.GetTypeByMetadataName(typeof(ICalculator).FullName),
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName),
                    compilation.GetTypeByMetadataName(typeof(IFormattable).FullName),
                });

            var syntax = await document.GetSyntaxRootAsync();

            document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax);

            await AssertCode.NoErrorsAsync(document);
        }

        async Task CanGenerateProxy(string language, Type type)
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
        }
    }
}
