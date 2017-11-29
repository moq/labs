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
    public class OutdatedStuntAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ST002";

        static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.OutdatedStuntAnalyzer_Title), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.OutdatedStuntAnalyzer_Description), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.OutdatedStuntAnalyzer_Message), Resources.ResourceManager, typeof(Resources));

        const string Category = "Build";
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.InvocationExpression);
        }

        static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var symbol = context.Compilation.GetSemanticModel(context.Node.SyntaxTree).GetSymbolInfo(context.Node);
            if (symbol.Symbol?.Kind == SymbolKind.Method)
            {
                var generator = context.Compilation.GetTypeByMetadataName(typeof(StuntGeneratorAttribute).FullName);
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
                        if (stunt != null)
                        {
                            // See if the symbol has any diagnostics associated
                            var diag = context.Compilation.GetDiagnostics();
                            var stuntPath = stunt.Locations[0].GetLineSpan().Path;

                            bool IsStuntLoc(Location loc) => loc.IsInSource && loc.GetLineSpan().Path == stuntPath;

                            var stuntDiag = diag
                                .Where(d => IsStuntLoc(d.Location) || d.AdditionalLocations.Any(IsStuntLoc))
                                .Where(d => d.Id == "CS0535");

                            if (stuntDiag.Any())
                            {
                                // If there are compilation errors, we should update the proxy.
                                var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(),
                                    new[] { new KeyValuePair<string, string>("Name", name) }.ToImmutableDictionary(),
                                    name);

                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                }
            }
        }
    }
}