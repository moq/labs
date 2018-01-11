using System;
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
        [Fact(Skip = "Can't get Roslyn to properly build and run analyzers + codefix, keep getting build errors after applying code fixes.")]
        public async Task ReportDiagnosticForRecursiveMock()
        {
            var code = @"
using Moq;

namespace Recursive
{
    public interface IRecursiveRoot
    {
        IRecursiveBranch Branch { get; }
    }

    public interface IRecursiveBranch
    {
        IRecursiveLeaf Leaf { get; }
    }

    public interface IRecursiveLeaf
    {
        string Name { get; set; }
    }

    public class RecursiveMocks
    {
        public RecursiveMocks()
        {
            var mock = Mock.Of<IRecursiveRoot>();

            mock.Branch.Leaf.Name.Returns(""foo"");
        }
    }
}
";
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp, includeMockApi: true);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            AssertCode.NoErrors(compilation);

            //var doc = project.AddDocument("code.cs", SourceText.From(code));
            var doc = workspace.AddDocument(DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                "code.cs",
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create()))));

            compilation = await doc.Project.GetCompilationAsync(TimeoutToken(5));

            AssertCode.NoErrors(compilation);

            var withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new MockGeneratorAnalyzer()));

            var diagnostics = (await withAnalyzers.GetAnalyzerDiagnosticsAsync())
                .OrderBy(d => d.Location.SourceSpan.Start)
                .ToArray();

            // Apply first codefix in document order
            var provider = new GenerateMockCodeFix();
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(doc, diagnostics[0], (a, d) => actions.Add(a), TimeoutToken(5));

            await provider.RegisterCodeFixesAsync(context);

            var solution = actions
                .SelectMany(x => x.GetOperationsAsync(TimeoutToken(2)).Result)
                .OfType<ApplyChangesOperation>()
                .First()
                .ChangedSolution;

            doc = solution.GetDocument(doc.Id);
            var analyzer = new RecursiveMockAnalyzer();
            compilation = await doc.Project.GetCompilationAsync(TimeoutToken(5));
            var diag = compilation.GetDiagnostics(TimeoutToken(5));

            AssertCode.NoErrors(compilation);

            withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        }
    }
}
