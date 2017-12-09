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
        public string GetName(INamedTypeSymbol baseType, ImmutableArray<INamedTypeSymbol> implementedInterfaces)
        {
            if (baseType == null)
                return string.Join("", implementedInterfaces.OrderBy(x => x.Name).Select(x => x.Name)) + NameSuffix;

            // NOTE: we sort the names the same way the StuntGenerator.ValidateTypes does.
            // There should be another analyzer that forces the first T to be the one with 
            // a class type, if any.
            if (baseType.TypeKind == TypeKind.Class)
                return baseType.Name + string.Join("", implementedInterfaces.OrderBy(x => x.Name).Select(x => x.Name)) + NameSuffix;
            else
                return string.Join("", implementedInterfaces.Concat(new[] { baseType }).OrderBy(x => x.Name).Select(x => x.Name)) + NameSuffix;
        }

        /// <summary>
        /// The full type name for the given (optional) base type and implemented interfaces.
        /// </summary>
        public string GetFullName(INamedTypeSymbol baseType, ImmutableArray<INamedTypeSymbol> implementedInterfaces)
            => Namespace + "." + GetName(baseType, implementedInterfaces);
    }
}
