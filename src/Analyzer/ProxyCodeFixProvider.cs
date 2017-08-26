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
using System;
using System.Collections.Generic;

namespace Moq.Analyzer
{
    // NOTE: all behavior is implemented in the ProxyCodeFix and the derived 
    // exported classes just provide the custom title depending on the 
    // analyzer that detected either a missing or outdated proxy.

    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(GenerateProxyCodeFix)), Shared]
    public class GenerateProxyCodeFix : ProxyCodeFixProvider
    {
        static readonly FixAllProvider fixAll = new ProxyFixAllProvider(Resources.GenerateProxyCodeFix_FixAllTitle);

        [ImportingConstructor]
        public GenerateProxyCodeFix([Import(AllowDefault = true)] ICodeAnalysisServices analysisServices)
            : base(analysisServices, Strings.GenerateProxyCodeFix.Title) { }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get => ImmutableArray.Create(MissingProxyAnalyzer.DiagnosticId);
        }

        public sealed override FixAllProvider GetFixAllProvider() => fixAll;
    }

    [ExportCodeFixProvider(LanguageNames.CSharp, new[] { LanguageNames.VisualBasic }, Name = nameof(GenerateProxyCodeFix)), Shared]
    public class UpdateProxyCodeFix : ProxyCodeFixProvider
    {
        static readonly FixAllProvider fixAll = new ProxyFixAllProvider(Resources.UpdateProxyCodeFix_FixAllTitle);

        [ImportingConstructor]
        public UpdateProxyCodeFix([Import(AllowDefault = true)] ICodeAnalysisServices analysisServices)
            : base(analysisServices, Strings.UpdateProxyCodeFix.Title) { }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get => ImmutableArray.Create(OutdatedProxyAnalyzer.DiagnosticId);
        }

        public sealed override FixAllProvider GetFixAllProvider() => fixAll;
    }

    public abstract class ProxyCodeFixProvider : CodeFixProvider
    {
        ICodeAnalysisServices analysisServices;
        string title;

        [ImportingConstructor]
        public ProxyCodeFixProvider([Import(AllowDefault = true)] ICodeAnalysisServices analysisServices, string title)
        {
            this.analysisServices = analysisServices;
            this.title = title;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

            // Find the invocation identified by the diagnostic.
            var invocation =
                (SyntaxNode)token.Parent.AncestorsAndSelf().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>().FirstOrDefault() ??
                (SyntaxNode)token.Parent.AncestorsAndSelf().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.InvocationExpressionSyntax>().FirstOrDefault();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                new ProxyCodeAction(title, context.Document, diagnostic, analysisServices),
                diagnostic);
        }
    }
}