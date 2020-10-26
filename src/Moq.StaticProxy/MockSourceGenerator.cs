using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq.Sdk;
using Stunts;
using Stunts.CodeAnalysis;

namespace Moq
{
    /// <summary>
    /// Generates mocks by inspecting the current compilation for 
    /// invocations to methods annotated with [MockGenerator].
    /// </summary>
    [Generator]
    public class MockSourceGenerator : StuntSourceGenerator
    {
        protected override StuntDocumentGenerator DocumentGenerator => new MockDocumentGenerator();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Execute(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.CheckDebugger(nameof(MockSourceGenerator));

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MoqAnalyzerDir", out var analyerDir))
                AddResolveDirectory(analyerDir);

            base.Execute(context);
        }

        protected override void OnExecute(GeneratorExecutionContext context, StuntDocumentGenerator generator, IEnumerable<INamedTypeSymbol[]> candidates)
        {
            var additional = new List<INamedTypeSymbol[]>();

            if (context.SyntaxReceiver is IEnumerable receivers && 
                receivers.OfType<RecursiveMockSyntaxReceiver>().FirstOrDefault() is var recursive)
            {
                var generatorAttr = context.Compilation.GetTypeByMetadataName(generator.GeneratorAttribute.FullName!);
                var moqmodule = context.Compilation.GetTypeByMetadataName("Moq.IMoq")!.ContainingModule;
                var sdkmodule = context.Compilation.GetTypeByMetadataName(typeof(IMock).FullName!)!.ContainingModule;

                // If we can't know what's the attribute that annotates mock generators, we can't do anything.
                if (generatorAttr != null)
                {
                    foreach (var node in recursive.CandidateNodes)
                    {
                        var semantic = context.Compilation.GetSemanticModel(node.SyntaxTree);
                        if (semantic == null)
                            break;

                        var flow = semantic.AnalyzeDataFlow(node);

                        // There are two possible flows:
                        // mock.Prop.Method().Returns(...): this is recursive "normal" flow In
                        // mock.Setup(x => x.Prop.Method()).Returns(...): 
                        //      this is a recursive "read outside" flow: the flow into the recursive expression
                        //      is actually the lambda parameter, not useful. But the "read outside" is the actual 
                        //      mock variable where the Setup is being performed, which is what we need.
                        bool IsMockFlow(ImmutableArray<ISymbol> data) =>
                            data.Length == 1 &&
                            data[0].DeclaringSyntaxReferences.Length == 1 &&
                            data[0].DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) is VariableDeclaratorSyntax variable &&
                            variable.Initializer?.Value is InvocationExpressionSyntax create &&
                            semantic!.GetSymbolInfo(create, context.CancellationToken).Symbol is IMethodSymbol method &&
                            method.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, generatorAttr));

                        // Detect if the variable being accessed was initialized from a generator method call
                        if (!IsMockFlow(flow.DataFlowsIn) && !IsMockFlow(flow.ReadOutside))
                            continue;

                        var symbol = semantic.GetSymbolInfo(node);
                        // TODO: see if we need to consider symbol.CandidateSymbols too
                        if (symbol.Symbol == null ||
                            SymbolEqualityComparer.Default.Equals(symbol.Symbol.ContainingModule, moqmodule) ||
                            SymbolEqualityComparer.Default.Equals(symbol.Symbol.ContainingModule, sdkmodule))
                            continue;

                        // We only process recursive property and method accesses
                        if (symbol.Symbol.Kind != SymbolKind.Property &&
                            symbol.Symbol.Kind != SymbolKind.Method)
                            continue;

                        var methodSymbol = symbol.Symbol as IMethodSymbol;
                        var propertySymbol = symbol.Symbol as IPropertySymbol;

                        // Extension methods are not considered for mocking
                        if (methodSymbol?.IsExtensionMethod == true ||
                            // void methods can't result in a recursive mock either
                            methodSymbol?.ReturnsVoid == true)
                            continue;

                        var type = (methodSymbol?.ReturnType ?? propertySymbol?.Type) as INamedTypeSymbol;
                        if (type != null && type.CanBeIntercepted() == false)
                            continue;

                        additional.Add(new[] { type! });
                    }
                }
            }

            // TODO: concat the symbols for recursive mocks
            base.OnExecute(context, generator, candidates.Concat(additional));
        }

        protected override IEnumerable<ISyntaxReceiver> CreateSyntaxReceivers()
            => base.CreateSyntaxReceivers().Concat(new[] { new RecursiveMockSyntaxReceiver() });

        class RecursiveMockSyntaxReceiver : ISyntaxReceiver
        {
            public List<SyntaxNode> CandidateNodes { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression) ||
                    syntaxNode.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression))
                    CandidateNodes.Add(syntaxNode);
            }
        }
    }
}