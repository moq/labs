using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using static TestHelpers;

namespace Stunts.Tests
{
    public class VisualBasicParameterFixupTest
    {
        [Fact]
        public async Task WhenParameterNameMatchesMethodNameParameterIsRenamed()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.VisualBasic);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var document = await new StuntGenerator().GenerateDocumentAsync(project, new[]
            {
                compilation.GetTypeByMetadataName(typeof(ICustomFormatter).FullName),
            }, TimeoutToken(5));

            var syntax = await document.GetSyntaxRootAsync();
            document = project.AddDocument("proxy.vb", syntax, filePath: Path.GetTempFileName());

            await AssertCode.NoErrorsAsync(document);
        }
    }
}
