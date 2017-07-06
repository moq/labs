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
    [ExportCodeFixProvider(LanguageNames.CSharp, new [] { LanguageNames.VisualBasic }, Name = nameof(MissingProxyCodeFix)), Shared]
    public class MissingProxyCodeFix : CodeFixProvider
    {
        ICodeAnalysisServices analysisServices;

        [ImportingConstructor]
        public MissingProxyCodeFix([Import(AllowDefault = true)] ICodeAnalysisServices analysisServices) => this.analysisServices = analysisServices;

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get => ImmutableArray.Create(MissingProxyAnalyzer.DiagnosticId);
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
                    title: Strings.MissingProxyCodeFix.Title,
                    createChangedSolution: c => GenerateProxyAsync(context.Document, invocation, c),
                    equivalenceKey: nameof(MissingProxyCodeFix)),
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

                var code = syntax.NormalizeWhitespace().ToFullString();
                var workspace = document.Project.Solution.Workspace;

                var projectId = ProjectId.CreateNewId();
                var solution = workspace.CurrentSolution.AddProject(ProjectInfo.Create(
                    projectId,
                    VersionStamp.Create(),
                    "Proxy",
                    "Proxy",
                    document.Project.Language,
                    compilationOptions: document.Project.CompilationOptions,
                    parseOptions: document.Project.ParseOptions,
                    documents: document.Project.Documents
                        .Where(d => d.Id != document.Id)
                        .Select(d => DocumentInfo.Create(
                            DocumentId.CreateNewId(projectId), d.Name, d.Folders, d.SourceCodeKind, filePath: d.FilePath)),
                    projectReferences: document.Project.ProjectReferences,
                    metadataReferences: document.Project.MetadataReferences));

                var extension = document.Project.Language == LanguageNames.CSharp ? ".cs" : ".vb";
                var file = Path.Combine(Path.GetDirectoryName(document.Project.FilePath), ProxyGenerator.ProxyNamespace, name + extension);
                var docId = DocumentId.CreateNewId(projectId);
                solution = solution.AddDocument(DocumentInfo.Create(
                    docId,
                    Path.GetFileName(file),
                    filePath: file,
                    folders: new[] { "Proxies" },
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create()))));

                var proxy = solution.GetDocument(docId);

                proxy = await ProxyGenerator.ApplyVisitors(proxy, analysisServices, cancellationToken);
                // This is somewhat expensive, but since we're adding it to the user' solution, we might 
                // as well make it look great ;)
                proxy = await Simplifier.ReduceAsync(proxy);
                proxy = await Formatter.FormatAsync(proxy);
                syntax = await proxy.GetSyntaxRootAsync();

                code = syntax.NormalizeWhitespace().ToFullString();

                return document.Project.AddDocument(Path.GetFileName(file),
                    code,
                    new[] { "Proxies" },
                    file)
                    .Project.Solution;
            }
            else
            {
                return document.Project.Solution;
            }
        }
    }
}
