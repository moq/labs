using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Stunts;

namespace Microsoft.CodeAnalysis
{
    public static class DocumentExtensions
    {
        static Lazy<ImmutableArray<DiagnosticAnalyzer>> builtInAnalyzers = new Lazy<ImmutableArray<DiagnosticAnalyzer>>(() =>
            MefHostServices
                .DefaultAssemblies
                .SelectMany(x => x.GetTypes()
                .Where(t => !t.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(t)))
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                // Add our own.
                .Concat(new[] { new OverridableMembersAnalyzer() })
                .ToImmutableArray());

        /// <summary>
        /// Applies the given named code fix to a document.
        /// </summary>
        public static async Task<Document> ApplyCodeFixAsync(this Document document, string codeFixName, CancellationToken cancellationToken = default(CancellationToken))
        {
            // If we request and process ALL codefixes at once, we'll get one for each 
            // diagnostics, which is one per non-implemented member of the interface/abstract 
            // base class, so we'd be applying unnecessary fixes after the first one.
            // So we re-retrieve them after each Apply, which will leave only the remaining 
            // ones.
            var codeFixes = await GetCodeFixes(document, codeFixName, cancellationToken).ConfigureAwait(false);
            while (codeFixes.Length != 0)
            {
                var operations = await codeFixes[0].Action.GetOperationsAsync(cancellationToken);
                ApplyChangesOperation operation;
                if ((operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault()) != null)
                {
                    document = operation.ChangedSolution.GetDocument(document.Id);
                    // Retrieve the codefixes for the updated doc again.
                    codeFixes = await GetCodeFixes(document, codeFixName, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // If we got no applicable code fixes, exit the loop and move on to the next codefix.
                    break;
                }
            }

            return document;
        }

        static async Task<ImmutableArray<ICodeFix>> GetCodeFixes(
            Document document, string codeFixName, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var provider = GetCodeFixProvider(document, codeFixName);
            if (provider == null)
                return ImmutableArray<ICodeFix>.Empty;

            var compilation = await document.Project.GetCompilationAsync(cancellationToken);
            // TODO: should we allow extending the set of built-in analyzers being added?
            var analyerCompilation = compilation.WithAnalyzers(builtInAnalyzers.Value, cancellationToken: cancellationToken);
            var allDiagnostics = await analyerCompilation.GetAllDiagnosticsAsync(cancellationToken);
            var diagnostics = allDiagnostics
                .Where(x => provider.FixableDiagnosticIds.Contains(x.Id))
                // Only consider the diagnostics raised by the target document.
                .Where(d =>
                    d.Location.Kind == LocationKind.SourceFile &&
                    d.Location.GetLineSpan().Path == document.FilePath);

            var codeFixes = new List<ICodeFix>();
            foreach (var diagnostic in diagnostics)
            {
                await provider.RegisterCodeFixesAsync(
                    new CodeFixContext(document, diagnostic,
                    (action, diag) => codeFixes.Add(new CodeFixAdapter(action, diag, codeFixName)),
                    cancellationToken));
            }

            return codeFixes.ToImmutableArray();
        }

        static CodeFixProvider GetCodeFixProvider(Document document, string codeFixName)
            => codeFixName == nameof(OverrideAllMembersCodeFix) ? new OverrideAllMembersCodeFix() :
                document.Project.Solution.Workspace.Services.HostServices.GetExports<CodeFixProvider, CodeChangeProviderMetadata>()
                    .Where(x => x.Metadata.Languages.Contains(document.Project.Language) && x.Metadata.Name == codeFixName)
                    .Select(x => x.Value)
                    .FirstOrDefault();
    }
}
