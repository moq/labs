using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Stunts
{
    /// <summary>
    /// Analyzer that flags types that have overridable members as 
    /// exposed by <see cref="RoslynInternals.GetOverridableMembers"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class OverridableMembersAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The diagnostics identifier exposed by this analyzer, <c>ST999</c>.
        /// </summary>
        public const string DiagnosticId = "ST999";
        /// <summary>
        /// The category for the diagnostics reported by this analyzer, <c>Build</c>.
        /// </summary>
        public const string Category = "Build";

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            nameof(OverridableMembersAnalyzer),
            nameof(OverridableMembersAnalyzer),
            Category,
            DiagnosticSeverity.Hidden, isEnabledByDefault: true);

        /// <summary>
        /// Reports the only supported rule by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <summary>
        /// Registers the analyzer for both C# and VB class declarations.
        /// </summary>
        /// <param name="context"></param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.ClassBlock);
        }

        static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var symbol = context.Compilation.GetSemanticModel(context.Node.SyntaxTree).GetDeclaredSymbol(context.Node) as INamedTypeSymbol;
            if (symbol == null)
                return;

            var overridable = RoslynInternals.GetOverridableMembers(symbol, context.CancellationToken);

            if (context.Node.Language == LanguageNames.VisualBasic)
                overridable = overridable.Where(x => x.MetadataName != "Finalize")
                    // VB doesn't support overriding events (yet). See https://github.com/dotnet/vblang/issues/63
                    .Where(x => x.Kind != SymbolKind.Event)
                    .ToImmutableArray();

            if (overridable.Length != 0)
            {
                var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
