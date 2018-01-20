using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Stunts
{
    public static class DiagnosticsExtensions
    {
        static readonly HashSet<string> compilationErrorIds = new HashSet<string>
        {
            // C# non-implemented abstract member
            "CS0534",
            // C# non-implemented interface member
            "CS0535",
            // VB non-implemented abstract member
            "BC30610",
            // VB non-implemented interface member
            "BC30149",
        };

        /// <summary>
        /// Gets the diagnostics that represent build errors that happen when generated
        /// code is out of date.
        /// </summary>
        public static Diagnostic[] GetCompilationErrors(this Compilation compilation) 
            => compilation.GetDiagnostics().Where(d => compilationErrorIds.Contains(d.Id)).ToArray();

        /// <summary>
        /// Checks if any of the diagnostics provided applies to the given symbol.
        /// </summary>
        public static bool HasDiagnostic(this INamedTypeSymbol symbol, Diagnostic[] diagnostics)
        {
            var symbolPath = symbol.Locations[0].GetLineSpan().Path;
            bool isSymbolLoc(Location loc) => loc.IsInSource && loc.GetLineSpan().Path == symbolPath;

            return diagnostics
                .Any(d => isSymbolLoc(d.Location) || d.AdditionalLocations.Any(isSymbolLoc));
        }

    }
}
