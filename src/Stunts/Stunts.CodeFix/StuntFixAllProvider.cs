using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Stunts.Properties;

namespace Stunts
{
    class StuntFixAllProvider : FixAllProvider
    {
        readonly string diagnosticId;
        readonly Func<Document, Diagnostic, CodeAction> codeActionFactory;

        public StuntFixAllProvider(string diagnosticId, Func<Document, Diagnostic, CodeAction> codeActionFactory)
        {
            this.diagnosticId = diagnosticId;
            this.codeActionFactory = codeActionFactory;
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

                        title = string.Format(Resources.StuntFixAllProvider_Title, diagnosticId, fixAllContext.Document.Name);
                        break;
                    }

                case FixAllScope.Project:
                    {
                        Project project = fixAllContext.Project;
                        var diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project);
                        if (diagnostics.Length > 0)
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(project, diagnostics));

                        title = string.Format(Resources.StuntFixAllProvider_Title, diagnosticId, fixAllContext.Project.Name);
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

                        title = string.Format(Resources.StuntFixAllProvider_Title, diagnosticId, 
                            fixAllContext.Solution.FilePath == null ? 
                            nameof(FixAllScope.Solution) : 
                            Path.GetFileName(fixAllContext.Solution.FilePath));
                        break;
                    }

                case FixAllScope.Custom:
                    return null;
                default:
                    break;
            }

            return new FixAllProxiesCodeAction(title, fixAllContext.Solution, diagnosticsToFix, codeActionFactory);
        }

        class FixAllProxiesCodeAction : CodeAction
        {
            readonly string title;
            readonly Solution solution;
            readonly List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix;
            readonly Func<Document, Diagnostic, CodeAction> codeActionFactory;

            public FixAllProxiesCodeAction(string title, 
                Solution solution, 
                List<KeyValuePair<Project, ImmutableArray<Diagnostic>>> diagnosticsToFix, 
                Func<Document, Diagnostic, CodeAction> codeActionFactory)
            {
                this.title = title;
                this.solution = solution;
                this.diagnosticsToFix = diagnosticsToFix;
                this.codeActionFactory = codeActionFactory;
            }

            public override string Title => title;

            protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
            {
                var currentSolution = solution;
                var fixerTasks = new List<Task>();
                var addedDocs = new ConcurrentBag<Document>();
                var updatedDocs = new ConcurrentBag<Document>();

                foreach (var pair in diagnosticsToFix)
                {
                    var project = solution.GetProject(pair.Key.Id);
                    Debug.Assert(project != null, "Failed to get project from solution.");
                    var diagnostics = pair.Value;

                    var group = diagnostics.GroupBy(d => d.Properties["TargetFullName"]);
                    foreach (var diag in group)
                    {
                        var diagnostic = diag.First();
                        var document = diagnostic.Location.IsInSource ?
                            project.Documents.FirstOrDefault(doc => doc.FilePath == diagnostic.Location.SourceTree.FilePath) :
                            project.GetDocument(diagnostic.Location.SourceTree);
                        
                        Debug.Assert(document != null, "Failed to locate document from diagnostic.");

                        fixerTasks.Add(new CodeFixer(document, diagnostic, addedDocs, updatedDocs, codeActionFactory)
                            .RunAsync(cancellationToken));
                    }
                }

                await Task.WhenAll(fixerTasks).ConfigureAwait(false);

                foreach (var addedDoc in addedDocs)
                {
                    var addedText = await addedDoc.GetTextAsync(cancellationToken).ConfigureAwait(false);
                    var addedVersion = await addedDoc.GetTextVersionAsync(cancellationToken).ConfigureAwait(false);
                    currentSolution = currentSolution.AddDocument(DocumentInfo.Create(
                        addedDoc.Id, addedDoc.Name, addedDoc.Folders,
                        addedDoc.SourceCodeKind,
                        TextLoader.From(TextAndVersion.Create(addedText, addedVersion, addedDoc.FilePath)),
                        addedDoc.FilePath));
                }

                foreach (var updatedDoc in updatedDocs)
                {
                    var updatedText = await updatedDoc.GetTextAsync(cancellationToken).ConfigureAwait(false);
                    currentSolution = currentSolution.WithDocumentText(updatedDoc.Id, updatedText);
                }

                return currentSolution;
            }

            class CodeFixer
            {
                private readonly Document document;
                private readonly Diagnostic diagnostic;
                private readonly ConcurrentBag<Document> addedDocs;
                private readonly ConcurrentBag<Document> updatedDocs;
                private readonly Func<Document, Diagnostic, CodeAction> codeActionFactory;

                public CodeFixer(Document document, Diagnostic diagnostic, ConcurrentBag<Document> addedDocs, ConcurrentBag<Document> updatedDocs, Func<Document, Diagnostic, CodeAction> codeActionFactory)
                {
                    this.document = document;
                    this.diagnostic = diagnostic;
                    this.addedDocs = addedDocs;
                    this.updatedDocs = updatedDocs;
                    this.codeActionFactory = codeActionFactory;
                }

                public async Task RunAsync(CancellationToken cancellationToken)
                {
                    // NOTE: stunts don't need to update the source document where the diagnostic 
                    // was reported, so we don't need any of the document updating stuff that 
                    // we need when applying code fixers in the moq/stunt codegen itself on its own 
                    // document. So we just apply the workspace changes and that's it.
                    var codeAction = codeActionFactory(document, diagnostic);
                    var operations = await codeAction.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
                    var applyChanges = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
                    if (applyChanges != null)
                    {
                        var changes = applyChanges.ChangedSolution.GetChanges(document.Project.Solution);
                        foreach (var change in changes.GetProjectChanges())
                        {
                            foreach (var addedId in change.GetAddedDocuments())
                            {
                                addedDocs.Add(applyChanges.ChangedSolution.GetDocument(addedId));
                            }
                            foreach (var changedId in change.GetChangedDocuments(true))
                            {
                                updatedDocs.Add(applyChanges.ChangedSolution.GetDocument(changedId));
                            }
                        }
                    }
                }
            }
        }
    }
}