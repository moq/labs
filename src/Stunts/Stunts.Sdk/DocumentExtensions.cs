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
using Microsoft.CodeAnalysis.Text;
using Stunts;

namespace Microsoft.CodeAnalysis
{
    /// <summary>
    /// Extensions methods for to <see cref="Document"/>.
    /// </summary>
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
        /// Exposes the built-in analyzers, discovered via reflection.
        /// </summary>
        // TODO: see if this should be moved elsewhere.
        public static ImmutableArray<DiagnosticAnalyzer> BuiltInAnalyzers => builtInAnalyzers.Value;

        /// <summary>
        /// Applies the given named code fix to a document.
        /// </summary>
        public static async Task<Document> ApplyCodeFixAsync(this Document document, string codeFixName, ImmutableArray<DiagnosticAnalyzer> analyzers = default, CancellationToken cancellationToken = default)
        {
            // If we request and process ALL codefixes at once, we'll get one for each 
            // diagnostics, which is one per non-implemented member of the interface/abstract 
            // base class, so we'd be applying unnecessary fixes after the first one.
            // So we re-retrieve them after each Apply, which will leave only the remaining 
            // ones.
            var codeFixes = await GetCodeFixes(document, codeFixName, analyzers, cancellationToken);
            while (codeFixes.Length != 0)
            {
                var operations = await codeFixes[0].Action.GetOperationsAsync(cancellationToken);
                ApplyChangesOperation operation;
                if ((operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault()) != null)
                {
                    // According to https://github.com/DotNetAnalyzers/StyleCopAnalyzers/pull/935 and 
                    // https://github.com/dotnet/roslyn-sdk/issues/140, Sam Harwell mentioned that we should 
                    // be forcing a re-parse of the document syntax tree at this point. 
                    document = await operation.ChangedSolution.GetDocument(document.Id).RecreateDocumentAsync(cancellationToken);
                    // Retrieve the codefixes for the updated doc again.
                    codeFixes = await GetCodeFixes(document, codeFixName, analyzers, cancellationToken);
                }
                else
                {
                    // If we got no applicable code fixes, exit the loop and move on to the next codefix.
                    break;
                }
            }

            return document;
        }

        /// <summary>
        /// Forces recreation of the text of a document.
        /// </summary>
        public static async Task<Document> RecreateDocumentAsync(this Document document, CancellationToken cancellationToken)
        {
            var newText = await document.GetTextAsync(cancellationToken);
            newText = newText.WithChanges(new TextChange(new TextSpan(0, 0), " "));
            newText = newText.WithChanges(new TextChange(new TextSpan(0, 1), string.Empty));
            return document.WithText(newText);
        }

        static async Task<ImmutableArray<ICodeFix>> GetCodeFixes(
            Document document, string codeFixName,
            ImmutableArray<DiagnosticAnalyzer> analyzers = default, CancellationToken cancellationToken = default)
        {
            var provider = GetCodeFixProvider(document, codeFixName);
            if (provider == null)
                return ImmutableArray<ICodeFix>.Empty;

            var compilation = await document.Project.GetCompilationAsync(cancellationToken);
            // TODO: should we allow extending the set of built-in analyzers being added?
            if (analyzers.IsDefaultOrEmpty)
                analyzers = builtInAnalyzers.Value;

            var analyerCompilation = compilation.WithAnalyzers(analyzers, cancellationToken: cancellationToken);
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
                document.Project.Solution.Workspace.Services.HostServices
                    .GetExports<CodeFixProvider, IDictionary<string, object>>()
                    .Where(x =>
                        x.Metadata.ContainsKey("Languages") && x.Metadata.ContainsKey("Name") &&
                        x.Metadata["Languages"] is string[] languages &&
                        languages.Contains(document.Project.Language) &&
                        x.Metadata["Name"] is string name && name == codeFixName)
                    .Select(x => x.Value)
                    .FirstOrDefault();
    }
}
