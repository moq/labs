using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

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
    public class StuntGeneratorAnalyzer : DiagnosticAnalyzer
    {
        private readonly NamingConvention naming;
        private readonly bool recursive;
        private Type generatorAttribute;

        /// <summary>
        /// Instantiates the analyzer with the default <see cref="NamingConvention"/> and 
        /// for method invocations annotated with <see cref="StuntGeneratorAttribute"/>.
        /// </summary>
        public StuntGeneratorAnalyzer() : this(new NamingConvention(), typeof(StuntGeneratorAttribute)) { }

        /// <summary>
        /// Customizes the analyzer by specifying a custom <see cref="NamingConvention"/> and 
        /// <see cref="generatorAttribute"/> to lookup in method invocations.
        /// </summary>
        protected StuntGeneratorAnalyzer(NamingConvention naming, Type generatorAttribute, bool recursive = false)
        {
            this.naming = naming;
            this.generatorAttribute = generatorAttribute;
            this.recursive = recursive;
        }

        /// <summary>
        /// Provides metadata about the missing generated type diagnostic.
        /// </summary>
        public virtual DiagnosticDescriptor MissingDiagnostic => StuntDiagnostics.MissingStunt;

        /// <summary>
        /// Provides metadata about the outdated generated type diagnostic.
        /// </summary>
        public virtual DiagnosticDescriptor OutdatedDiagnostic => StuntDiagnostics.OutdatedStunt;

        /// <summary>
        /// Returns the single <see cref="MissingDiagnostic"/> this analyzer supports.
        /// </summary>
        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            // NOTE: this creates the return value at this point because both Missing and Outdated 
            // descriptors can be overridden as a customization point.
            get { return ImmutableArray.Create(MissingDiagnostic, OutdatedDiagnostic); }
        }

        /// <summary>
        /// Registers the analyzer to take action on method invocation expressions.
        /// </summary>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            // Get the matching symbol for the given generator attribute from the current compilation.
            var generator = context.Compilation.GetTypeByMetadataName(generatorAttribute.FullName);
            if (generator == null)
            {
                // This may be an extender authoring error, but another analyzer should ensure proper 
                // metadata references exist. Typically, the same NuGet package that adds this analyzer
                // also adds the required assembly references, so this should never happen anyway.
                return;
            }

            var symbol = context.Compilation.GetSemanticModel(context.Node.SyntaxTree).GetSymbolInfo(context.Node);
            if (symbol.Symbol?.Kind != SymbolKind.Method)
            {
                return;
            }

            var method = (IMethodSymbol)symbol.Symbol;
            if (method.GetAttributes().Any(x => x.AttributeClass == generator) &&
                // We don't generate anything if generator is applied to a non-generic method.
                !method.TypeArguments.IsDefaultOrEmpty &&
                method.TypeArguments.TryValidateGeneratorTypes(out _))
            {
                var name = naming.GetFullName(method.TypeArguments.OfType<INamedTypeSymbol>());
                var compilationErrors = new Lazy<Diagnostic[]>(() => context.Compilation.GetCompilationErrors());
                HashSet<string> recursiveSymbols;

                if (recursive)
                {
                    // Collect recursive symbols to generate/update as needed.
                    recursiveSymbols = new HashSet<string>(method.TypeArguments.OfType<INamedTypeSymbol>().InterceptableRecursively()
                        .Where(x =>
                        {
                            var candidate = context.Compilation.GetTypeByMetadataName(naming.GetFullName(new[] { x }));
                            return candidate == null || candidate.HasDiagnostic(compilationErrors.Value);
                        })
                        .Select(x => x.ToFullMetadataName()));
                }
                else
                {
                    recursiveSymbols = new HashSet<string>();
                }

                // See if the stunt already exists
                var stunt = context.Compilation.GetTypeByMetadataName(name);
                if (stunt == null)
                {
                    var diagnostic = Diagnostic.Create(
                        MissingDiagnostic,
                        context.Node.GetLocation(),
                        new Dictionary<string, string>
                        {
                            { "TargetFullName", name },
                            { "Symbols", string.Join("|", method.TypeArguments
                                .OfType<INamedTypeSymbol>().Select(x => x.ToFullMetadataName())) },
                            // By passing the detected recursive symbols to update/generate, 
                            // we avoid doing all the work we already did during analysis. 
                            // The code action can therefore simply act on them, without 
                            // further inquiries to the compilation.
                            { "RecursiveSymbols", string.Join("|", recursiveSymbols) },
                        }.ToImmutableDictionary(),
                        name);

                    context.ReportDiagnostic(diagnostic);
                }
                else
                {
                    // See if the symbol has any compilation error diagnostics associated
                    if (stunt.HasDiagnostic(compilationErrors.Value) ||
                       (recursive && recursiveSymbols.Any()))
                    {
                        var location = stunt.Locations.Length == 1
                            ? stunt.Locations[0].GetLineSpan().Path
                            : stunt.Locations.FirstOrDefault(loc =>
                                loc.GetLineSpan().Path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
                                loc.GetLineSpan().Path.EndsWith(".g.vb", StringComparison.OrdinalIgnoreCase))
                                ?.GetLineSpan().Path
                                ?? "";

                        // Any outdated files should be automatically updated on the next build, 
                        // so signal this to the build targets with a text file that the targets 
                        // can use to determine a pre-compile analysis and codefix phase is needed.
                        if (context.Options.GetCodeFixSettings().TryGetValue("IntermediateOutputPath", out var intermediateOutputPath))
                        {
                            File.WriteAllText(Path.Combine(intermediateOutputPath, "AutoCodeFixBeforeCompile.flag"), "");
                        }

                        // If there are compilation errors, we should update the proxy.
                        var diagnostic = Diagnostic.Create(
                            OutdatedDiagnostic,
                            context.Node.GetLocation(),
                            new Dictionary<string, string>
                            {
                                { "TargetFullName", name },
                                { "Location", location },
                                { "Symbols", string.Join("|", method.TypeArguments
                                    .OfType<INamedTypeSymbol>().Select(x => x.ToFullMetadataName())) },
                                // We pass the same recursive symbols in either case. The 
                                // Different diagnostics exist only to customize the message 
                                // displayed to the user.
                                { "RecursiveSymbols", string.Join("|", recursiveSymbols) },
                            }.ToImmutableDictionary(),
                            name);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
