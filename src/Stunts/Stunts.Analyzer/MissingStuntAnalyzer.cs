using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stunts.Properties;

namespace Stunts
{
    // TODO: F#
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MissingStuntAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ST001";

        static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MissingStuntAnalyzer_Title), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MissingStuntAnalyzer_Description), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MissingStuntAnalyzer_Message), Resources.ResourceManager, typeof(Resources));

        const string Category = "Build";
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.InvocationExpression);
        }

        static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var generator = context.Compilation.GetTypeByMetadataName(typeof(StuntGeneratorAttribute).FullName);
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
                    var baseType = method.TypeArguments.OfType<INamedTypeSymbol>().FirstOrDefault();
                    var interfaces = method.TypeArguments.OfType<INamedTypeSymbol>().Skip(1).ToImmutableArray();
                    // TODO: if this isn't true it means somehow we have a generic method invocation with
                    // no generic type arguments that are INamedTypeSymbols, which I think shouldn't apply?
                    if (baseType != null)
                    {
                        var name = StuntNaming.StuntsNamespace + "." + StuntSymbolNaming.GetName(baseType, interfaces);

                        // See if the stunt already exists
                        var stunt = context.Compilation.GetTypeByMetadataName(name);
                        if (stunt == null)
                        {
                            var diagnostic = Diagnostic.Create(
                                Rule, 
                                context.Node.GetLocation(),
                                //new[] { new KeyValuePair<string, string>("Name", name) }.ToImmutableDictionary(),
                                name);

                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}
