using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Stunts
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class OverridableMembersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ST001";
        public const string Category = "Build";

        static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            nameof(OverridableMembersAnalyzer),
            nameof(OverridableMembersAnalyzer),
            Category,
            DiagnosticSeverity.Hidden, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

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

            var overridable = symbol.GetOverridableMembers(context.CancellationToken);
            if (context.Node.Language == LanguageNames.VisualBasic)
                overridable = overridable.Where(x => x.MetadataName != "Finalize").ToImmutableArray();

            if (overridable.Length != 0)
            {
                var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
