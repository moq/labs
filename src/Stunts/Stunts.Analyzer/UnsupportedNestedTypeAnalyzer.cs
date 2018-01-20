using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Stunts.Properties;

namespace Stunts
{
    /// <summary>
    /// Analyzes source code looking for method invocations to methods annotated with 
    /// the <see cref="StuntGeneratorAttribute"/> and reports unsupported nested types for 
    /// codegen.
    /// </summary>
    // TODO: this is a stop-gap measure until we can figure out why the ImplementInterfaces codefix
    // is not returning any code actions for the CSharpScaffold (maybe VB too?) when the type to 
    // be implemented is a nested interface :(. In the IDE, the code action is properly available after 
    // generating the (non-working, since the interface isn't implemented) mock class. 
    // One possible workaround to not prevent the scenario altogether would be to leverage the 
    // IImplementInterfaceService ourselves by duplicating the (simple) behavior of the CSharpImplementInterfaceCodeFixProvider 
    // (see http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp.Features/ImplementInterface/CSharpImplementInterfaceCodeFixProvider.cs,fd395d6b5f6a7dd3)
    // and wrapping the code action in our own that would run the rewriters right after invoking the built-in one. I fear, 
    // however, that the same reason the codefix isn't showing up when we ask for it, will cause that service to return 
    // an empty list of code actions too. 
    // It may be costly to investigate, and it doesn't seem like a core scenario anyway. 
    // Another workaround might be to let the user manually implement stunts/mocks and detect the presence of 
    // the IStunt interface and offer the rewriting codefix. This might be interesting too in general.
    // TODO: F#
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class UnsupportedNestedTypeAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            "ST009",
            new ResourceString(nameof(Resources.UnsupportedNestedTypeAnalyzer_Title)),
            new ResourceString(nameof(Resources.UnsupportedNestedTypeAnalyzer_Message)), 
            "Build", 
            DiagnosticSeverity.Error, 
            true,
            new ResourceString(nameof(Resources.UnsupportedNestedTypeAnalyzer_Description)));

        readonly NamingConvention naming;
        Type generatorAttribute;

        /// <summary>
        /// Instantiates the analyzer with the default <see cref="NamingConvention"/> and 
        /// for method invocations annotated with <see cref="StuntGeneratorAttribute"/>.
        /// </summary>
        public UnsupportedNestedTypeAnalyzer() : this(new NamingConvention(), typeof(StuntGeneratorAttribute)) { }

        /// <summary>
        /// Customizes the analyzer by specifying a custom <see cref="NamingConvention"/> and 
        /// <see cref="generatorAttribute"/> to lookup in method invocations.
        /// </summary>
        protected UnsupportedNestedTypeAnalyzer(NamingConvention naming, Type generatorAttribute)
        {
            this.naming = naming;
            this.generatorAttribute = generatorAttribute;
        }

        /// <summary>
        /// Returns the single descriptor this analyzer supports.
        /// </summary>
        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

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
                // This may be an extender authoring error, but another analyzer should ensure proper 
                // metadata references exist. Typically, the same NuGet package that adds this analyzer
                // also adds the required assembly references, so this should never happen anyway.
                return;

            var symbol = context.Compilation.GetSemanticModel(context.Node.SyntaxTree).GetSymbolInfo(context.Node);
            if (symbol.Symbol?.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)symbol.Symbol;
                if (method.GetAttributes().Any(x => x.AttributeClass == generator) && 
                    // We don't generate anything if generator is applied to a non-generic method.
                    !method.TypeArguments.IsDefaultOrEmpty)
                    // Skip generic method definitions since they are typically usability overloads 
                    // like Mock.Of<T>(...)
                    // TODO: doesn't seem like this would be needed?
                    //!method.TypeArguments.Any(x => x.Kind == SymbolKind.TypeParameter))
                {
                    var args = method.TypeArguments.OfType<INamedTypeSymbol>().Where(t => t.ContainingType != null).ToArray();
                    if (args.Length != 0)
                    {
                        var diagnostic = Diagnostic.Create(
                            descriptor,
                            context.Node.GetLocation(),
                            string.Join(", ", args.Select(t => t.ContainingType.Name + "." + t.Name)));

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
