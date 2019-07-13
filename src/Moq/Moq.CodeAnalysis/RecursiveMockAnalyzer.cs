using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;
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

        /// <summary>
        /// Gest the supported diagnostics, which are <see cref="MockDiagnostics.Missing"/> 
        /// <see cref="MockDiagnostics.Outdated"/>.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(MockDiagnostics.Missing, MockDiagnostics.Outdated);

        /// <inheritdoc />
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

            var lambda = context.Node.Ancestors().OfType<CS.LambdaExpressionSyntax>().Cast<SyntaxNode>().FirstOrDefault() ??
                context.Node.Ancestors().OfType<VB.LambdaExpressionSyntax>().Cast<SyntaxNode>().FirstOrDefault();

            SymbolInfo setupInfo = default;

            // This could be one of two setup scenarios: Setup(x => ...) or a typed ref/out Setup(x.TryFoo).
            // If we cannot find a lambda, find the setup invocation where the node is just an argument
            if (lambda != null)
            {
                var member = lambda.Ancestors().FirstOrDefault(node =>
                    node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression) ||
                    node.IsKind(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.InvocationExpression));
                if (member == null)
                    return;

                setupInfo = semantic.GetSymbolInfo(member);
            }
            else
            {
                var argumentsNode = context.Node.Ancestors().FirstOrDefault(node =>
                    node.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArgumentList) ||
                    node.IsKind(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.ArgumentList));

                if (argumentsNode == null)
                    return;

                // The arguments parent is the InvocationExpression
                setupInfo = semantic.GetSymbolInfo(argumentsNode.Parent);
            }

            if (setupInfo.Symbol == null ||
                !setupInfo.Symbol.GetAttributes().Any(x => x.AttributeClass == scope))
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