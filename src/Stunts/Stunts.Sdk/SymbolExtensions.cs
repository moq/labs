using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Stunts
{
    public static class SymbolExtensions
    {
        const string TaskFullName = "System.Threading.Tasks.Task";
        const string ValueTupleFullName = "System.ValueTuple";

        public static ImmutableHashSet<INamedTypeSymbol> InterceptableRecursively(this IEnumerable<INamedTypeSymbol> symbols)
        {
            var set = new HashSet<INamedTypeSymbol>();
            foreach (var symbol in symbols)
            {
                CollectSymbols(symbol, set);
            }
            return set.ToImmutableHashSet();
        }

        static void CollectSymbols(INamedTypeSymbol symbol, HashSet<INamedTypeSymbol> set)
        {
            var members = symbol.GetMembers();
            // We only collect method return types and property types.
            // TODO: collecting byref/out  method parameters would be 
            // needed too to set their default values to mocks also...
            var candidates = members
                .OfType<IMethodSymbol>()
                .Where(s => !s.ReturnsVoid && s.ReturnType.CanBeIntercepted())
                .Select(s => s.ReturnType)
                .Concat(members

                .OfType<IPropertySymbol>()
                .Where(s => s.Type.CanBeIntercepted())
                .Select(s => s.Type))
                .OfType<INamedTypeSymbol>();

            foreach (var candidate in candidates)
            {
                if (set.Add(candidate))
                    CollectSymbols(candidate, set);
            }
        }

        /// <summary>
        /// Whether the given type symbol is either an interface or a non-abstract class.
        /// </summary>
        public static bool CanBeIntercepted(this ITypeSymbol symbol)
            => symbol.CanBeReferencedByName &&
              !symbol.ToString().StartsWith(TaskFullName, StringComparison.Ordinal) &&
              !symbol.ToString().StartsWith(ValueTupleFullName, StringComparison.Ordinal) &&
              (symbol.TypeKind == TypeKind.Interface ||
              (symbol?.TypeKind == TypeKind.Class && symbol?.IsAbstract == true));

        /// <summary>
        /// Gets the full metadata name of the given symbol.
        /// </summary>
        public static string ToFullMetadataName(this INamedTypeSymbol symbol)
            => (symbol.ContainingNamespace == null || symbol.ContainingNamespace.IsGlobalNamespace ?
                "" : symbol.ContainingNamespace + ".") +
                (symbol.ContainingType != null ?
                 symbol.ContainingType.MetadataName + "+" : "") +
                symbol.MetadataName;
    }
}
