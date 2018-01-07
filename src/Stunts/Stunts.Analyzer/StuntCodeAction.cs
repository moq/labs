using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace Stunts
{
    public class StuntCodeAction : CodeAction
    {
        string title;
        Document document;
        readonly Diagnostic diagnostic;
        readonly SyntaxNode invocation;
        readonly NamingConvention naming;

        public StuntCodeAction(string title, Document document, Diagnostic diagnostic, SyntaxNode invocation, NamingConvention naming)
        {
            this.title = title;
            this.document = document;
            this.diagnostic = diagnostic;
            this.invocation = invocation;
            this.naming = naming;
        }

        public override string Title => title;

        protected virtual StuntGenerator CreateGenerator(NamingConvention naming) => new StuntGenerator(naming);

        protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document.Project.Solution;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel == null)
                return document.Project.Solution;

            var symbol = semanticModel.GetSymbolInfo(invocation);
            if (symbol.Symbol?.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)symbol.Symbol;
                var generator = SyntaxGenerator.GetGenerator(document.Project);
                var stunts = CreateGenerator(naming);
                var symbols = method.TypeArguments.OfType<INamedTypeSymbol>();
                var solution = document.Project.Solution;

                var compilation = await document.Project.GetCompilationAsync(cancellationToken);
                var name = naming.GetFullName(method.TypeArguments.OfType<INamedTypeSymbol>());
                var stunt = compilation.GetTypeByMetadataName(name);

                solution = await CreateStunt(symbols, generator, stunts, cancellationToken);
                // Update the document for the changed solution.
                document = solution.GetDocument(document.Id);

                var recursiveSymbols = diagnostic.Properties["RecursiveSymbols"]
                    .Split('|')
                    .Select(compilation.GetTypeByMetadataName)
                    .Where(x => x != null)
                    .ToArray();

                foreach (var recursive in recursiveSymbols)
                {
                    solution = await CreateStunt(new[] { recursive }, generator, stunts, cancellationToken);
                    // Update the document for the changed solution.
                    document = solution.GetDocument(document.Id);
                }

                return solution;
            }

            return document.Project.Solution;
        }

        async Task<Solution> CreateStunt(IEnumerable<INamedTypeSymbol> symbols, SyntaxGenerator generator, StuntGenerator stunts, CancellationToken cancellationToken)
        {
            var (name, syntax) = stunts.CreateStunt(symbols, generator);

            // TODO: F#
            var extension = document.Project.Language == LanguageNames.CSharp ? ".cs" : ".vb";
            var file = Path.Combine(Path.GetDirectoryName(document.Project.FilePath), naming.Namespace, name + extension);
            var folders = naming.Namespace.Split('.');

            var stuntDoc = document.Project.Documents.FirstOrDefault(d => d.Name == Path.GetFileName(file) && d.Folders.SequenceEqual(folders));
            if (stuntDoc == null)
            {
                stuntDoc = document.Project.AddDocument(Path.GetFileName(file),
                    syntax,
                    folders,
                    file);
            }
            else
            {
                stuntDoc = stuntDoc.WithSyntaxRoot(syntax);
            }

            stuntDoc = await stunts.ApplyProcessors(stuntDoc, cancellationToken).ConfigureAwait(false);
            // This is somewhat expensive, but since we're adding it to the user' solution, we might 
            // as well make it look great ;)
            stuntDoc = await Simplifier.ReduceAsync(stuntDoc).ConfigureAwait(false);
            if (document.Project.Language != LanguageNames.VisualBasic)
                stuntDoc = await Formatter.FormatAsync(stuntDoc).ConfigureAwait(false);

            syntax = await stuntDoc.GetSyntaxRootAsync().ConfigureAwait(false);

            return stuntDoc.WithSyntaxRoot(syntax).Project.Solution;
        }
    }
}
