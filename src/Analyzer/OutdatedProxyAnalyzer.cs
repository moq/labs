using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq.Analyzer.Properties;
using Moq.Proxy;

namespace Moq.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class OutdatedProxyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MOQ002";

        static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.OutdatedProxyAnalyzer_Title), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.OutdatedProxyAnalyzer_Description), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.OutdatedProxyAnalyzer_Message), Resources.ResourceManager, typeof(Resources));

        const string Category = "Build";

        static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var symbol = context.Compilation.GetSemanticModel(context.Node.SyntaxTree).GetSymbolInfo(context.Node);
            if (symbol.Symbol?.Kind == SymbolKind.Method)
            {
                var generator = context.Compilation.GetTypeByMetadataName(typeof(ProxyGeneratorAttribute).FullName);
                var method = (IMethodSymbol)symbol.Symbol;
                if (method.GetAttributes().Any(x => x.AttributeClass == generator) &&
                    // Skip generic method definitions since they are typically usability overloads 
                    // like Mock.Of<T>(...)
                    !method.TypeArguments.Any(x => x.Kind == SymbolKind.TypeParameter))
                {
                    var name = ProxyGenerator.GetProxyFullName(method.TypeArguments);

                    // See if the proxy already exists
                    var proxy = context.Compilation.GetTypeByMetadataName(name);
                    if (proxy != null)
                    {
                        // See if the symbol has any diagnostics associated
                        var diag = context.Compilation.GetDiagnostics();
                        var proxyPath = proxy.Locations[0].GetLineSpan().Path;

                        Func<Location, bool> isProxyLoc = (loc) => loc.IsInSource && loc.GetLineSpan().Path == proxyPath;

                        var proxyDiag = diag
                            .Where(d => isProxyLoc(d.Location) || d.AdditionalLocations.Any(a => isProxyLoc(a)))
                            .Where(d => d.Id == "CS0535");

                        if (proxyDiag.Any())
                        {
                            // If there are compilation errors, we should update the proxy.
                            var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), name);

                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}
