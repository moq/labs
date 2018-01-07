using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using static TestHelpers;

namespace Stunts.Tests
{
    class SymbolExtensionsTests
    {
        [InlineData("System.IDisposable")]
        [InlineData("System.Threading.Tasks.Task`1")]
        [InlineData("System.Environment+SpecialFolder")]
        [Theory]
        public async Task ToFullMetadataRoundTrip(string metadataName)
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var compilation = await project.GetCompilationAsync();
            var symbol = compilation.GetTypeByMetadataName(metadataName);

            Assert.NotNull(symbol);

            var fullMetadata = symbol.ToFullMetadataName();

            var symbol2 = compilation.GetTypeByMetadataName(fullMetadata);

            Assert.NotNull(symbol2);

            Assert.Same(symbol, symbol2);
        }

        [Fact]
        public async Task ToFullMetadata()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var code = @"public class Foo { }";
            var doc = project.AddDocument("code.cs", SourceText.From(code));
            var compilation = await doc.Project.GetCompilationAsync();
            var symbol = compilation.GetTypeByMetadataName("Foo");

            Assert.NotNull(symbol);

            var fullMetadata = symbol.ToFullMetadataName();

            var symbol2 = compilation.GetTypeByMetadataName(fullMetadata);

            Assert.NotNull(symbol2);

            Assert.Same(symbol, symbol2);
        }
    }
}
