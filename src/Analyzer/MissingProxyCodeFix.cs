using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host;
using System.IO;
using Microsoft.CodeAnalysis.Text;
using Moq.Analyzer.Properties;
using Moq.Proxy;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Simplification;

namespace Moq.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingProxyCodeFix)), Shared]
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
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the invocation identified by the diagnostic.
            var invocation = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Strings.MissingProxyCodeFix.Title,
                    createChangedSolution: c => GenerateProxyAsync(context.Document, invocation, c),
                    equivalenceKey: nameof(MissingProxyCodeFix)),
                diagnostic);
        }

        async Task<Solution> GenerateProxyAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var symbol = semanticModel.GetSymbolInfo(invocation);
            if (symbol.Symbol?.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)symbol.Symbol;

                var generator = SyntaxGenerator.GetGenerator(document.Project);
                var (name, syntax) = ProxyGenerator.CreateProxy(method.TypeArguments, generator);

                var code = syntax.NormalizeWhitespace().ToFullString();

                var workspace = new AdhocWorkspace(document.Project.Solution.Workspace.Services.HostServices, "Proxy");
                var info = ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "Proxy",
                    "Proxy",
                    document.Project.Language,
                    compilationOptions: document.Project.CompilationOptions,
                    parseOptions: document.Project.ParseOptions,
                    metadataReferences: document.Project.MetadataReferences);
                var project = workspace.AddProject(info);
                var file = Path.Combine(Path.GetDirectoryName(document.Project.FilePath), ProxyGenerator.ProxyNamespace, name + ".cs");
                var proxy = project.AddDocument(Path.GetFileName(file),
                    SourceText.From(code),
                    new[] { "Proxies" },
                    file);

                proxy = await ProxyGenerator.ApplyVisitors(proxy, analysisServices, cancellationToken);
                proxy = await Simplifier.ReduceAsync(proxy);
                syntax = await proxy.GetSyntaxRootAsync();

                var output = syntax.NormalizeWhitespace().ToFullString();

                return document.Project.AddDocument(Path.GetFileName(file),
                    output,
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
