using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Stunts.Tests
{
    public class StuntDiscovererTests
    {
        ITestOutputHelper output;

        public StuntDiscovererTests(ITestOutputHelper output) => this.output = output;

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanDiscoverDocumentStunts(string languageName)
        {
            var (workspace, project) = CreateWorkspaceAndProject(languageName);

            var code = languageName == LanguageNames.CSharp ?
                File.ReadAllText(@"StuntDiscovererTests.Compile.cs") :
                File.ReadAllText(@"StuntDiscovererTests.Compile.vb");

            var document = project.AddDocument("test", code);
            await AssertCode.NoErrorsAsync(document);

            var stunts = await document.DiscoverStuntsAsync();

            Assert.Single(stunts);
        }

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanDiscoverProjectStunts(string languageName)
        {
            var (workspace, project) = CreateWorkspaceAndProject(languageName);

            var code = languageName == LanguageNames.CSharp ?
                File.ReadAllText(@"StuntDiscovererTests.Compile.cs") :
                File.ReadAllText(@"StuntDiscovererTests.Compile.vb");

            var document = project.AddDocument("test", code);
            await AssertCode.NoErrorsAsync(document);

            var stunts = await document.Project.DiscoverStuntsAsync();

            Assert.Single(stunts);
        }
    }
}
