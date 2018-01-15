using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Stunts.Properties;

namespace Stunts
{
    /// <summary>
    /// Helper methods for dealing with <see cref="ISymbol"/>s.
    /// </summary>
    public static class SymbolExtensions
    {
        const string TaskFullName = "System.Threading.Tasks.Task";
        const string ValueTupleFullName = "System.ValueTuple";

        /// <summary>
        /// Retrieves the distinct set of symbols that can be intercepted from the 
        /// given list, by inspecting each one and all its properties and methods 
        /// recursively looking for symbols that can be intercepted via codegen.
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
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
        /// Sorts and validates types so that the base class is the first and the remaining 
        /// interface implementations are the rest.
        /// </summary>
        /// <exception cref="ArgumentException">The list of types is invalid for a generator invocation.</exception>
        public static (INamedTypeSymbol baseType, ImmutableArray<INamedTypeSymbol> additionalInterfaces) ValidateGeneratorTypes(this IEnumerable<INamedTypeSymbol> types)
        {
            var symbols = types.ToArray();
            if (symbols.Length == 0)
                throw new ArgumentException(Strings.SymbolRequired);

            var baseType = default(INamedTypeSymbol);
            var additionalInterfaces = default(IEnumerable<INamedTypeSymbol>);
            if (symbols[0].TypeKind == TypeKind.Class)
            {
                baseType = symbols[0];
                if (symbols.Skip(1).Any(x => x.TypeKind == TypeKind.Class))
                    throw new ArgumentException(Strings.WrongBaseType(string.Join(",", symbols.Select(x => x.Name))));
                if (baseType.IsSealed)
                    throw new ArgumentException(Strings.InvalidSealedBaseType);
                if (symbols.Skip(1).Any(x => x.TypeKind != TypeKind.Interface))
                    throw new ArgumentException(Strings.InvalidStuntTypes(string.Join(",", symbols.Select(x => x.Name))));

                additionalInterfaces = symbols.Skip(1);
            }
            else
            {
                if (symbols.Any(x => x.TypeKind == TypeKind.Class))
                    throw new ArgumentException(Strings.WrongBaseType(string.Join(",", symbols.Select(x => x.Name))));
                if (symbols.Any(x => x.TypeKind != TypeKind.Interface))
                    throw new ArgumentException(Strings.InvalidStuntTypes(string.Join(",", symbols.Select(x => x.Name))));

                additionalInterfaces = symbols;
            }

            return (baseType, additionalInterfaces.OrderBy(x => x.Name).ToImmutableArray());
        }

        public static bool TryValidateGeneratorTypes(this IEnumerable<ITypeSymbol> types, 
            out (INamedTypeSymbol baseType, ImmutableArray<INamedTypeSymbol> additionalInterfaces) result)
        {
            var symbols = types.ToArray();
            result = default;
            if (symbols.Length == 0)
                return false;

            var baseType = default(INamedTypeSymbol);
            var additionalInterfaces = default(IEnumerable<INamedTypeSymbol>);
            if (symbols[0].TypeKind == TypeKind.Class)
            {
                baseType = (INamedTypeSymbol)symbols[0];
                if (types.Skip(1).Any(x => x.TypeKind == TypeKind.Class))
                    return false;
                if (baseType.IsSealed)
                    return false;
                if (types.Skip(1).Any(x => x.TypeKind != TypeKind.Interface))
                    return false;

                additionalInterfaces = types.Skip(1).Cast<INamedTypeSymbol>();
            }
            else
            {
                if (symbols.Any(x => x.TypeKind == TypeKind.Class))
                    return false;
                if (symbols.Any(x => x.TypeKind != TypeKind.Interface))
                    return false;

                additionalInterfaces = types.Cast<INamedTypeSymbol>();
            }

            result = (baseType, additionalInterfaces.OrderBy(x => x.Name).ToImmutableArray());
            return true;
        }

        /// <summary>
        /// Whether the given type symbol is either an interface or a non-sealed class.
        /// </summary>
        public static bool CanBeIntercepted(this ITypeSymbol symbol)
            => symbol.CanBeReferencedByName &&
              !symbol.ToString().StartsWith(TaskFullName, StringComparison.Ordinal) &&
              !symbol.ToString().StartsWith(ValueTupleFullName, StringComparison.Ordinal) &&
              (symbol.TypeKind == TypeKind.Interface ||
              (symbol?.TypeKind == TypeKind.Class && symbol?.IsSealed == false));

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
