using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq.Sdk;
using Stunts;
using Xunit;
using static TestHelpers;

namespace Moq
{
    // TODO: can't get Roslyn to compile generated code while running the tests :(
    class DynamicMock
    {
        MockGenerator generator = new MockGenerator(new MockNamingConvention());
        Project project;
        readonly string language;

        public DynamicMock(string language) => this.language = language;

        public async Task<T> CreateAsync<T>(params object[] args)
        {
            var compilation = await GenerateAsync(typeof(T));
            var assembly = compilation.Emit();
            var name = MockNaming.GetFullName(typeof(T), new Type[0]);
            var type = assembly.GetType(name, true);

            return (T)Activator.CreateInstance(type, args);
        }

        async Task<Compilation> GenerateAsync(params Type[] types)
        {
            var project = await GetProjectAsync();
            var compilation = await project.GetCompilationAsync();
            var symbols = types.Select(t => compilation.GetTypeByMetadataName(t.FullName)).ToArray();
            var document = await generator.GenerateDocumentAsync(project, symbols, TimeoutToken(5));

            var syntax = await document.GetSyntaxRootAsync();
            document = project.AddDocument(MockNaming.GetName(types[0], types.Skip(1).ToArray()) + (language == LanguageNames.CSharp ? ".cs" : ".vb"), 
                syntax, 
                filePath: document.FilePath);

            await AssertCode.NoErrorsAsync(document);

            return await document.Project.GetCompilationAsync();
        }

        async Task<Project> GetProjectAsync()
        {
            if (project == null)
            {
                var (workspace, project) = CreateWorkspaceAndProject(language, includeStuntApi: false, includeMockApi: true);
                this.project = project.AddAnalyzerReference(new AnalyzerImageReference(new DiagnosticAnalyzer[] { new OverridableMembersAnalyzer() }.ToImmutableArray()));
                var compilation = await project.GetCompilationAsync(TimeoutToken(5));

                Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                    string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));
            }

            return project;
        }
    }
}
