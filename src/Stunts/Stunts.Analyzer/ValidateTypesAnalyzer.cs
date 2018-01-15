using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stunts.Properties;

namespace Stunts
{
    /// <summary>
    /// Analyzes source code looking for method invocations to methods annotated with 
    /// the <see cref="StuntGeneratorAttribute"/> and reports any missing or outdated 
    /// generated types using the given <see cref="NamingConvention"/> to locate the
    /// generated types.
    /// </summary>
    // TODO: F#
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ValidateTypesAnalyzer : DiagnosticAnalyzer
    {
        Type generatorAttribute;

        /// <summary>
        /// Instantiates the analyzer to validate invocations to methods 
        /// annotated with <see cref="StuntGeneratorAttribute"/>.
        /// </summary>
        public ValidateTypesAnalyzer() : this(typeof(StuntGeneratorAttribute)) { }

        /// <summary>
        /// Customizes the analyzer by specifying a custom <see cref="generatorAttribute"/> 
        /// to lookup in method invocations.
        /// </summary>
        protected ValidateTypesAnalyzer(Type generatorAttribute)
            => this.generatorAttribute = generatorAttribute;

        /// <summary>
        /// Returns the single <see cref="StuntDiagnostics.BaseTypeNotFirst"/> 
        /// diagnostic this analyer supports.
        /// </summary>
        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(
                StuntDiagnostics.BaseTypeNotFirst, 
                StuntDiagnostics.DuplicateBaseType,
                StuntDiagnostics.SealedBaseType);

        /// <summary>
        /// Registers the analyzer to take action on method invocation expressions.
        /// </summary>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.SimpleMemberAccessExpression);
        }

        void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            // Get the matching symbol for the given generator attribute from the current compilation.
            var generator = context.Compilation.GetTypeByMetadataName(generatorAttribute.FullName);
            if (generator == null)
                return;

            var symbol = context.Compilation.GetSemanticModel(context.Node.SyntaxTree).GetSymbolInfo(context.Node);
            if (symbol.Symbol?.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)symbol.Symbol;
                if (method.GetAttributes().Any(x => x.AttributeClass == generator) && 
                    !method.TypeArguments.IsDefaultOrEmpty)
                {
                    var classes = method.TypeArguments.Where(x => x.TypeKind == TypeKind.Class).ToArray();
                    if (classes.Length > 1)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            StuntDiagnostics.DuplicateBaseType,
                            context.Node.GetLocation()));
                    }
                    if (classes.Length == 1)
                    {
                        if (classes[0].IsSealed)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                StuntDiagnostics.SealedBaseType,
                                context.Node.GetLocation(),
                                classes[0].Name));
                        }
                        else if (method.TypeArguments.IndexOf(classes[0]) != 0)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                StuntDiagnostics.BaseTypeNotFirst,
                                context.Node.GetLocation(),
                                classes[0].Name));
                        }
                    }                    
                }
            }
        }
    }
}
