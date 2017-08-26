using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Simplification;
using Moq.Proxy;

namespace Moq.Analyzer
{
    class ProxyCodeAction : CodeAction
    {
        Document document;
        Diagnostic diagnostic;
        ICodeAnalysisServices analysisServices;

        public ProxyCodeAction(string title, Document document, Diagnostic diagnostic, ICodeAnalysisServices analysisServices)
        {
            Title = title;
            this.document = document;
            this.diagnostic = diagnostic;
            this.analysisServices = analysisServices;
        }

        public override string Title { get; }

        protected override async Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

            // Find the invocation identified by the diagnostic.
            var invocation =
                (SyntaxNode)token.Parent.AncestorsAndSelf().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>().FirstOrDefault() ??
                (SyntaxNode)token.Parent.AncestorsAndSelf().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.InvocationExpressionSyntax>().FirstOrDefault();

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbol = semanticModel.GetSymbolInfo(invocation);
            if (symbol.Symbol?.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)symbol.Symbol;
                var generator = SyntaxGenerator.GetGenerator(document.Project);
                var (name, syntax) = ProxyGenerator.CreateProxy(method.TypeArguments, generator);

                var extension = document.Project.Language == LanguageNames.CSharp ? ".cs" : ".vb";
                var file = Path.Combine(Path.GetDirectoryName(document.Project.FilePath), ProxyFactory.ProxyNamespace, name + extension);

                var proxyDoc = document.Project.Documents.FirstOrDefault(d => d.Name == Path.GetFileName(file) && d.Folders.SequenceEqual(new[] { "Proxies" }));
                if (proxyDoc == null)
                {
                    proxyDoc = document.Project.AddDocument(Path.GetFileName(file),
                        syntax,
                        new[] { "Proxies" },
                        file);
                }
                else
                {
                    proxyDoc = proxyDoc.WithSyntaxRoot(syntax);
                }

                proxyDoc = await ProxyGenerator.ApplyVisitors(proxyDoc, analysisServices, cancellationToken).ConfigureAwait(false);
                // This is somewhat expensive, but since we're adding it to the user' solution, we might 
                // as well make it look great ;)
                proxyDoc = await Simplifier.ReduceAsync(proxyDoc).ConfigureAwait(false);
                proxyDoc = await Formatter.FormatAsync(proxyDoc).ConfigureAwait(false);
                syntax = await proxyDoc.GetSyntaxRootAsync().ConfigureAwait(false);

                return proxyDoc.WithSyntaxRoot(syntax).Project.Solution;
            }

            return document.Project.Solution;
        }
    }
}
