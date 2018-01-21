using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using CSharp = Microsoft.CodeAnalysis.CSharp.Syntax;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Generates diagnostics for recursive mock invocations, so that 
    /// the <c>RecursiveMockBehavior</c> can properly locate the runtime 
    /// type for the recursive members accessed during a set up.
    /// </summary>
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
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.SimpleMemberAccessExpression);
        }

        void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var semantic = context.Compilation.GetSemanticModel(context.Node.SyntaxTree);
            var symbol = semantic.GetSymbolInfo(context.Node);
            // TODO: see if we need to consider symbol.CandidateSymbols too
            if (symbol.Symbol == null)
                return;

            // We only process recursive property and method accesses
            if (symbol.Symbol?.Kind != SymbolKind.Property &&
                symbol.Symbol?.Kind != SymbolKind.Method)
                return;

            var methodSymbol = symbol.Symbol as IMethodSymbol;
            var propertySymbol = symbol.Symbol as IPropertySymbol;

            // Extension methods are not considered for mocking
            if (methodSymbol?.IsExtensionMethod == true ||
                // void methods can't result in a recursive mock either
                methodSymbol?.ReturnsVoid == true)
                return;

            var generator = context.Compilation.GetTypeByMetadataName(generatorAttribute.FullName);
            var scope = context.Compilation.GetTypeByMetadataName(typeof(SetupScopeAttribute).FullName);

            // If we can't know what's the attribute that annotates mock generators, we can't do anything.
            if (generator == null || scope == null)
                return;

            var type = (methodSymbol?.ReturnType ?? propertySymbol.Type) as INamedTypeSymbol;
            if (type?.CanBeIntercepted() == false)
                return;

            var lambda = context.Node.Ancestors().OfType<CSharp.LambdaExpressionSyntax>().Cast<SyntaxNode>().FirstOrDefault() ??
                context.Node.Ancestors().OfType<VisualBasic.LambdaExpressionSyntax>().Cast<SyntaxNode>().FirstOrDefault();
            if (lambda == null)
                return;

            var member = lambda.Ancestors().OfType<CSharp.InvocationExpressionSyntax>().Cast<SyntaxNode>().FirstOrDefault() ??
                lambda.Ancestors().OfType<VisualBasic.InvocationExpressionSyntax>().Cast<SyntaxNode>().FirstOrDefault();
            if (member == null)
                return;

            var method = semantic.GetSymbolInfo(member);
            if (method.Symbol == null ||
                !method.Symbol.GetAttributes().Any(x => x.AttributeClass == scope))
                return;

            ReportDiagnostics(context, type);
        }

        static void ReportDiagnostics(SyntaxNodeAnalysisContext context, INamedTypeSymbol type)
        {
            var name = naming.GetFullName(new[] { type });
            // See if the mock already exists
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