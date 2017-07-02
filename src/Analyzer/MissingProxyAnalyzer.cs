using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq.Analyzer.Properties;
using Moq.Proxy;

namespace Moq.Analyzer 
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MissingProxyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MOQ001";

        static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MissingProxyAnalyzer_Title), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MissingProxyAnalyzer_Description), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MissingProxyAnalyzer_Message), Resources.ResourceManager, typeof(Resources));

        const string Category = "Build";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.InvocationExpression);
        }

        static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var generator = context.Compilation.GetTypeByMetadataName(typeof(ProxyGeneratorAttribute).FullName);
            if (generator == null)
                return;

            var symbol = context.Compilation.GetSemanticModel(context.Node.SyntaxTree).GetSymbolInfo(context.Node);
            if (symbol.Symbol?.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)symbol.Symbol;
                if (method.GetAttributes().Any(x => x.AttributeClass == generator) &&
                    // Skip generic method definitions since they are typically usability overloads 
                    // like Mock.Of<T>(...)
                    !method.TypeArguments.Any(x => x.Kind == SymbolKind.TypeParameter))
                {
                    var name = ProxyGenerator.GetProxyFullName(method.TypeArguments);

                    // See if the proxy already exists
                    var proxy = context.Compilation.GetTypeByMetadataName(name);
                    if (proxy == null)
                    {
                        var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), 
                            new [] { new KeyValuePair<string, string>("Name", name) }.ToImmutableDictionary(),
                            name);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
