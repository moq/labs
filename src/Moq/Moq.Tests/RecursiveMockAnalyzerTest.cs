using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using static TestHelpers;

namespace Moq.Tests
{
    public class RecursiveMockAnalyzerTest
    {
        [Fact]
        public async Task ReportDiagnosticForRecursiveMock()
        {
            var code = @"
using Moq;

namespace Recursive
{
    public interface IRecursiveRoot2
    {
        IRecursiveBranch2 Branch { get; }
    }

    public interface IRecursiveBranch2
    {
        IRecursiveLeaf2 Leaf { get; }
    }

    public interface IRecursiveLeaf2
    {
        string Name { get; set; }
    }

    public class RecursiveMocks
    {
        public RecursiveMocks()
        {
            var mock = Mock.Of<IRecursiveRoot2>();

            mock.Setup(m => m.Branch.Leaf.Name).Returns(""foo="");
            // mock.Branch.Leaf.Name.Returns(""foo"");
        }
    }
}
";
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp, includeMockApi: true);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            AssertCode.NoErrors(compilation);

            var doc = workspace.AddDocument(project, code);

            var exports = workspace.Services.HostServices.GetExports<CodeFixProvider, IDictionary<string, object>>()
                .Where(x =>
                    x.Metadata.ContainsKey("Languages") && x.Metadata.ContainsKey("Name") &&
                    x.Metadata["Languages"] is string[] languages &&
                    languages.Contains(doc.Project.Language) &&
                    x.Metadata["Name"] is string name && name == "ImplementInterface")
                .Select(x => x.Value)
                .FirstOrDefault();

            Assert.NotNull(exports);

            compilation = await doc.Project.GetCompilationAsync(TimeoutToken(5));

            AssertCode.NoErrors(compilation);

            // First we'll generate the mock from Mock.Of<T>
            doc = await workspace.ApplyCodeFixAsync<MockGeneratorAnalyzer, GenerateMockCodeFix>(doc, "MOQ001");

            await AssertCode.NoErrorsAsync(doc);

            var analyzer = new RecursiveMockAnalyzer();
            compilation = await doc.Project.GetCompilationAsync(TimeoutToken(5));
            var withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

            var diagnostics = (await withAnalyzers.GetAnalyzerDiagnosticsAsync())
                .Where(d => d.Id == "MOQ001")
                .OrderBy(d => d.Location.SourceSpan.Start)
                .ToArray();

            Assert.Equal(2, diagnostics.Length);

            doc = await workspace.ApplyCodeFixAsync<RecursiveMockAnalyzer, GenerateMockCodeFix>(doc, "MOQ001");
            doc = await workspace.ApplyCodeFixAsync<RecursiveMockAnalyzer, GenerateMockCodeFix>(doc, "MOQ001");

            compilation = await doc.Project.GetCompilationAsync(TimeoutToken(5));
            withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));
            diagnostics = (await withAnalyzers.GetAnalyzerDiagnosticsAsync())
                .Where(d => d.Id == "MOQ001")
                .OrderBy(d => d.Location.SourceSpan.Start)
                .ToArray();

            Assert.Empty(diagnostics);
        }
    }
}
