using System.Collections.Immutable;
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
        readonly Document document;
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

                var (name, syntax) = stunts.CreateStunt(
                    method.TypeArguments.OfType<INamedTypeSymbol>().First(),
                    method.TypeArguments.OfType<INamedTypeSymbol>().Skip(1).ToImmutableArray(), generator);

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

                stuntDoc = await stunts.ApplyVisitors(stuntDoc, cancellationToken).ConfigureAwait(false);
                // This is somewhat expensive, but since we're adding it to the user' solution, we might 
                // as well make it look great ;)
                stuntDoc = await Simplifier.ReduceAsync(stuntDoc).ConfigureAwait(false);
                stuntDoc = await Formatter.FormatAsync(stuntDoc).ConfigureAwait(false);
                syntax = await stuntDoc.GetSyntaxRootAsync().ConfigureAwait(false);

                return stuntDoc.WithSyntaxRoot(syntax).Project.Solution;
            }

            return document.Project.Solution;
        }
    }
}
