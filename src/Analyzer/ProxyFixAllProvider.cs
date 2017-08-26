using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Simplification;
using Moq.Proxy;

namespace Moq.Analyzer
{
    class ProxyFixAllProvider : FixAllProvider
    {
        string titleFormat;

        public ProxyFixAllProvider(string titleFormat) => this.titleFormat = titleFormat;

        public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
        {
            var diagnosticsToFix = new List<KeyValuePair<Project, ImmutableArray<Diagnostic>>>();
            string title = null;

            switch (fixAllContext.Scope)
            {
                case FixAllScope.Document:
                    {
                        ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document).ConfigureAwait(false);
                        diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));
                        title = string.Format(titleFormat, "document", fixAllContext.Document.Name);
                        break;
                    }

                case FixAllScope.Project:
                    {
                        Project project = fixAllContext.Project;
                        ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                        diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));
                        title = string.Format(titleFormat, "project", fixAllContext.Project.Name);
                        break;
                    }

                case FixAllScope.Solution:
                    {
                        foreach (Project project in fixAllContext.Solution.Projects)
                        {
                            ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(project, diagnostics));
                        }

                        title = string.Format(titleFormat, "solution", "");
                        break;
                    }

                case FixAllScope.Custom:
                    return null;
                default:
                    break;
            }

            return new FixAllProxiesCodeAction(title, fixAllContext.Solution, diagnosticsToFix);
        }

        class FixAllProxiesCodeAction : CodeAction
        {
            Solution solution;
            List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix;

            public FixAllProxiesCodeAction(string title, Solution solution, List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix)
            {
                Title = title;
                this.solution = solution;
                this.diagnosticsToFix = diagnosticsToFix;
            }

            public override string Title { get; }

            protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var analysisServices = solution.Workspace.Services.GetRequiredService<ICodeAnalysisServices>();

                foreach (var pair in diagnosticsToFix)
                {
                    Project project = pair.Key;
                    ImmutableArray<Diagnostic> diagnostics = pair.Value;

                    var group = diagnostics.GroupBy(d => d.Properties["Name"]);
                    foreach (var diag in group)
                    {
                        var diagnostic = diag.First();
                        var document = project.GetDocument(diagnostic.Location.SourceTree);
                        var codeAction = new ProxyCodeAction(Title, document, diagnostic, analysisServices);

                        var operations = await codeAction.GetOperationsAsync(cancellationToken);
                        ApplyChangesOperation operation;
                        if ((operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault()) != null)
                            solution = operation.ChangedSolution;
                    }
                }

                return solution;
            }
        }
    }
}
