using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Stunts
{
    internal static class RoslynInternals
    {
        internal static readonly MethodInfo getOverridableMembers = typeof(Workspace).Assembly
            .GetType("Microsoft.CodeAnalysis.Shared.Extensions.INamedTypeSymbolExtensions", false)
            ?.GetMethod("GetOverridableMembers", BindingFlags.Public | BindingFlags.Static);

        internal static readonly MethodInfo overrideAsync = 
            // 2.9+
            (typeof(Workspace).Assembly.GetType("Microsoft.CodeAnalysis.Shared.Extensions.ICodeDefinitionFactoryExtensions", false) ??
            // 2.11+
            typeof(Workspace).Assembly.GetType("Microsoft.CodeAnalysis.Shared.Extensions.SyntaxGeneratorExtensions", false))
            ?.GetMethod("OverrideAsync", BindingFlags.Public | BindingFlags.Static);

        internal static readonly MethodInfo addMemberDeclarationsAsync = typeof(Workspace).Assembly
            .GetType("Microsoft.CodeAnalysis.CodeGeneration.CodeGenerator", false)
            ?.GetMethod("AddMemberDeclarationsAsync", BindingFlags.Public | BindingFlags.Static);

        public static ImmutableArray<ISymbol> GetOverridableMembers(INamedTypeSymbol containingType, CancellationToken cancellationToken)
            => (ImmutableArray<ISymbol>)getOverridableMembers.Invoke(null, new object[] { containingType, cancellationToken });

        public static Task<ISymbol> OverrideAsync(SyntaxGenerator generator, ISymbol symbol, INamedTypeSymbol containingType, Document document, DeclarationModifiers? modifiersOpt = null, CancellationToken cancellationToken = default)
            => (Task<ISymbol>)overrideAsync.Invoke(null, new object[] { generator, symbol, containingType, document, modifiersOpt, cancellationToken });

        public static Task<Document> AddMemberDeclarationsAsync(Solution solution, INamedTypeSymbol destination, IEnumerable<ISymbol> members, CancellationToken cancellationToken = default)
            => (Task<Document>)addMemberDeclarationsAsync.Invoke(null, new object[] { solution, destination, members, null, cancellationToken });
    }
}
