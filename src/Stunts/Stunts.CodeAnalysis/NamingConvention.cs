using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
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
        public string GetName(IEnumerable<ITypeSymbol> symbols)
        {
            var builder = new StringBuilder();
            // First add the base class
            AddNames(builder, symbols.Where(x => x.TypeKind == TypeKind.Class));
            // Then the interfaces
            AddNames(builder, symbols.Where(x => x.TypeKind == TypeKind.Interface).OrderBy(x => x.Name));
            return builder.Append(NameSuffix).ToString();
        }

        static void AddNames(StringBuilder builder,  IEnumerable<ITypeSymbol> symbols)
        {
            foreach (var symbol in symbols)
            {
                builder.Append(symbol.Name);
                if (symbol is INamedTypeSymbol named && named.IsGenericType)
                {
                    builder.Append("Of");
                    AddNames(builder, named.TypeArguments);
                }
            }
        }

        /// <summary>
        /// The full type name for the given (optional) base type and implemented interfaces.
        /// </summary>
        public string GetFullName(IEnumerable<INamedTypeSymbol> symbols)
            => Namespace + "." + GetName(symbols);
    }
}
