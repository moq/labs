using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Stunts
{
    /// <summary>
    /// Exposes the naming conventions for stunt generation and instantiation.
    /// </summary>
    public static class StuntSymbolNaming
    {
        /// <summary>
        /// Gets the runtime stunt name from its base type and implemented interfaces.
        /// </summary>
        public static string GetName(INamedTypeSymbol baseType, ImmutableArray<INamedTypeSymbol> implementedInterfaces)
        {
            if (baseType == null)
                return string.Join("", implementedInterfaces.Select(x => x.Name)) + StuntNaming.StuntSuffix;

            return baseType.Name + string.Join("", implementedInterfaces.Select(x => x.Name)) + StuntNaming.StuntSuffix;
        }
    }
}