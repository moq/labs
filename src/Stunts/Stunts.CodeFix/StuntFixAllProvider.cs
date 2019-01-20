using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Stunts
{
    class StuntFixAllProvider : FixAllProvider
    {
        string titleFormat;
        string diagnosticId;

        public StuntFixAllProvider(string titleFormat, string diagnosticId)
        {
            this.titleFormat = titleFormat;
            this.diagnosticId = diagnosticId;
        }

        public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
        {
            var diagnosticsToFix = new List<KeyValuePair<Project, ImmutableArray<Diagnostic>>>();
            string title = null;

            switch (fixAllContext.Scope)
            {
                case FixAllScope.Document:
                    {
                        var diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document);
                        diagnostics = diagnostics.Where(d => d.Id == diagnosticId).ToImmutableArray();
                        if (diagnostics.Length > 0)
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));

                        title = string.Format(titleFormat, "document", fixAllContext.Document.Name);
                        break;
                    }

                case FixAllScope.Project:
                    {
                        Project project = fixAllContext.Project;
                        var diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project);
                        if (diagnostics.Length > 0)
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(project, diagnostics));

                        title = string.Format(titleFormat, "project", fixAllContext.Project.Name);
                        break;
                    }

                case FixAllScope.Solution:
                    {
                        foreach (var project in fixAllContext.Solution.Projects)
                        {
                            var diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project);
                            if (diagnostics.Length > 0)
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
            string title;
            Solution solution;
            List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix;

            public FixAllProxiesCodeAction(string title, Solution solution, List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix)
            {
                this.title = title;
                this.solution = solution;
                this.diagnosticsToFix = diagnosticsToFix;
            }

            public override string Title => title;

            protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var changedSolution = solution;
                foreach (var pair in diagnosticsToFix)
                {
                    var project = changedSolution.GetProject(pair.Key.Id);
                    var diagnostics = pair.Value;

                    var group = diagnostics.GroupBy(d => d.Properties["Name"]);
                    foreach (var diag in group)
                    {
                        var diagnostic = diag.First();
                        var document = project.GetDocument(diagnostic.Location.SourceTree);
                        var codeAction = new StuntCodeAction(Title, document, diagnostic, new NamingConvention());

                        var operations = await codeAction.GetOperationsAsync(cancellationToken);
                        ApplyChangesOperation operation;
                        if ((operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault()) != null)
                            changedSolution = operation.ChangedSolution;
                    }
                }

                return changedSolution;
            }
        }
    }
}
