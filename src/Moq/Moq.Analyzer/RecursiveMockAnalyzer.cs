using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
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

            var generator = context.Compilation.GetTypeByMetadataName(generatorAttribute.FullName);
            // If we can't know what's attribute that annotates mock generators, we can't do anything.
            if (generator == null)
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

            if (variable?.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.Parameter) == true &&
                (variable?.Parent?.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleLambdaExpression) == true ||
                 variable?.Parent?.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ParenthesizedLambdaExpression) == true))
            {
                // the member access is inside a lambda (i.e. Setup(m => ...)) invocation on the mock
                var lambda = (LambdaExpressionSyntax)variable.Parent;
                // the lambda is likely an argument to an invocation
                if (lambda.Parent?.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.Argument) == true &&
                    lambda.Parent?.Parent?.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArgumentList) == true &&
                    lambda.Parent?.Parent?.Parent?.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression) == true)
                {
                    // locate the variable the invocation is performed on.
                    var invocation = (InvocationExpressionSyntax)lambda.Parent.Parent.Parent;
                    if (invocation.Expression?.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression) == true)
                    {
                        memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
                        symbol = semantic.GetSymbolInfo(memberAccess);
                        owner = memberAccess.Expression;
                        variable = semantic.GetSymbolInfo(owner).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken);
                    }
                }
            }

            if (variable.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.VariableDeclarator))
            {
                // the member access is direct on the Mock.Of variable
                var initializer = (((VariableDeclaratorSyntax)variable).Initializer?.Value as InvocationExpressionSyntax)?.Expression;
                if (initializer != null &&
                    semantic.GetSymbolInfo(initializer).Symbol?.Kind == SymbolKind.Method &&
                    // given the previous comparison, .Symbol can't be null next
                    semantic.GetSymbolInfo(initializer).Symbol.GetAttributes().Any(x => x.AttributeClass == generator))
                {
                    ReportDiagnostics(context, type);
                }
            }
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
