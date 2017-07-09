using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Host;
using System.IO;
using Microsoft.CodeAnalysis.Text;
using Moq.Analyzer.Properties;
using Moq.Proxy;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace Moq.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(GenerateProxyCodeFix)), Shared]
    public class GenerateProxyCodeFix : ProxyCodeFix
    {
        [ImportingConstructor]
        public GenerateProxyCodeFix([Import(AllowDefault = true)] ICodeAnalysisServices analysisServices) 
            : base(analysisServices, Strings.GenerateProxyCodeFix.Title) { }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get => ImmutableArray.Create(MissingProxyAnalyzer.DiagnosticId);
        }
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(GenerateProxyCodeFix)), Shared]
    public class UpdateProxyCodeFix : ProxyCodeFix
    {
        [ImportingConstructor]
        public UpdateProxyCodeFix([Import(AllowDefault = true)] ICodeAnalysisServices analysisServices) 
            : base(analysisServices, Strings.UpdateProxyCodeFix.Title) { }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get => ImmutableArray.Create(OutdatedProxyAnalyzer.DiagnosticId);
        }
    }

    public abstract class ProxyCodeFix : CodeFixProvider
    {
        ICodeAnalysisServices analysisServices;
        string title;

        [ImportingConstructor]
        public ProxyCodeFix([Import(AllowDefault = true)] ICodeAnalysisServices analysisServices, string title)
        {
            this.analysisServices = analysisServices;
            this.title = title;
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            // TODO: implement
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
            var diagnostic = context.Diagnostics.First();
            var sourceToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

            // Find the invocation identified by the diagnostic.
            var invocation = 
                (SyntaxNode)sourceToken.Parent.AncestorsAndSelf().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>().FirstOrDefault() ??
                (SyntaxNode)sourceToken.Parent.AncestorsAndSelf().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.InvocationExpressionSyntax>().FirstOrDefault();
            
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: this.title,
                    createChangedSolution: c => GenerateProxyAsync(context.Document, invocation, c),
                    equivalenceKey: nameof(GenerateProxyCodeFix)),
                diagnostic);
        }

        async Task<Solution> GenerateProxyAsync(Document document, SyntaxNode invocation, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
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

                proxyDoc = await ProxyGenerator.ApplyVisitors(proxyDoc, analysisServices, cancellationToken);
                // This is somewhat expensive, but since we're adding it to the user' solution, we might 
                // as well make it look great ;)
                proxyDoc = await Simplifier.ReduceAsync(proxyDoc);
                proxyDoc = await Formatter.FormatAsync(proxyDoc);
                syntax = await proxyDoc.GetSyntaxRootAsync();
                //syntax = syntax.NormalizeWhitespace();

                return proxyDoc.WithSyntaxRoot(syntax).Project.Solution;
            }
            else
            {
                return document.Project.Solution;
            }
        }
    }
}
