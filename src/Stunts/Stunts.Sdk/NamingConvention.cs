using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Stunts
{
    /// <summary>
    /// Naming conventions used for analyzers, code fixes and code generation.
    /// </summary>
    public class NamingConvention
    {
        /// <summary>
        /// The namespace of the generated code.
        /// </summary>
        public virtual string Namespace => StuntNaming.Namespace;

        /// <summary>
        /// Suffix appended to the type name, i.e. <c>IFooStunt</c>.
        /// </summary>
        public virtual string NameSuffix => StuntNaming.NameSuffix;

        /// <summary>
        /// The type name to generate for the given (optional) base type and implemented interfaces.
        /// </summary>
        public string GetName(IEnumerable<INamedTypeSymbol> symbols)
            // NOTE: we sort the names the same way the StuntGenerator.ValidateTypes does.
            // There should be another analyzer that forces the first T to be the one with 
            // a class type, if any.
            => string.Join("", symbols
                .Where(x => x?.TypeKind == TypeKind.Class)
                .Concat(symbols
                    .Where(x => x?.TypeKind == TypeKind.Interface)
                    .OrderBy(x => x.Name))
                .Select(x => x?.Name)
                .Where(x => x != null)) + 
                NameSuffix;

        /// <summary>
        /// The full type name for the given (optional) base type and implemented interfaces.
        /// </summary>
        public string GetFullName(IEnumerable<INamedTypeSymbol> symbols)
            => Namespace + "." + GetName(symbols);
    }
}
