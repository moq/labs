using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Stunts;

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
            // TODO: concat the symbols for recursive mocks
            base.OnExecute(context, generator, candidates);
        }
    }
}