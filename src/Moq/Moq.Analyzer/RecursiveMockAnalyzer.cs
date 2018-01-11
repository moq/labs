using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq.Properties;
using Stunts;

namespace Moq
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class RecursiveMockAnalyzer : DiagnosticAnalyzer
    {
        static readonly NamingConvention naming = new MockNamingConvention();
        static readonly Type generatorAttribute = typeof(MockGeneratorAttribute);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
            => ImmutableArray.Create(MockDiagnostics.Missing, MockDiagnostics.Outdated);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression);
        }

        void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var semantic = context.Compilation.GetSemanticModel(context.Node.SyntaxTree);
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;
            var symbol = semantic.GetSymbolInfo(context.Node);
            if (symbol.Symbol?.Kind == SymbolKind.Method &&
                // Extension methods are the primary Moq syntax mechanism, we 
                // shouldn't be checking to generate code for the extension method 
                // classes themselves.
                ((IMethodSymbol)symbol.Symbol).IsExtensionMethod)
                return;

            var type = symbol.Symbol?.ContainingType;
            var owner = memberAccess.Expression;

            // Find variable declaration for this member access, to see if it's a 
            // mock we should check for.
            while (owner.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression)
            {
                memberAccess = (MemberAccessExpressionSyntax)owner;
                symbol = semantic.GetSymbolInfo(memberAccess);
                owner = memberAccess.Expression;
            }

            var variable = semantic.GetSymbolInfo(owner).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken);
            var initializer = ((variable as VariableDeclaratorSyntax)?.Initializer?.Value as InvocationExpressionSyntax)?.Expression;
            var generator = context.Compilation.GetTypeByMetadataName(generatorAttribute.FullName);

            if (initializer != null && generator != null && 
                semantic.GetSymbolInfo(initializer).Symbol?.Kind == SymbolKind.Method &&
                semantic.GetSymbolInfo(initializer).Symbol.GetAttributes().Any(x => x.AttributeClass == generator))
            {
                // The member access is from a Mock.
                var name = naming.GetFullName(new[] { type });
                // See if the stunt already exists
                var mock = context.Compilation.GetTypeByMetadataName(name);
                if (mock == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        MockDiagnostics.Missing,
                        context.Node.GetLocation(),
                        new Dictionary<string, string>
                        {
                            { "TargetFullName", name },
                            { "Symbols", type.ToFullMetadataName() },
                            { "RecursiveSymbols", "" },
                        }.ToImmutableDictionary(),
                        name));
                }
                else
                {
                    var compilationErrors = context.Compilation.GetCompilationErrors();
                    // See if the symbol has any compilation error diagnostics associated
                    if (mock.HasDiagnostic(compilationErrors))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            MockDiagnostics.Outdated,
                            context.Node.GetLocation(),
                            new Dictionary<string, string>
                            {
                                { "TargetFullName", name },
                                { "Symbols", type.ToFullMetadataName() },
                                { "RecursiveSymbols", "" },
                            }.ToImmutableDictionary(),
                            name));
                    }
                }
            }
        }
    }
}
