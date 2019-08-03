using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moq.Sdk
{
    /// <summary>
    /// Exposes the naming conventions for mock generation and instantiation.
    /// </summary>
    public static class MockNaming
    {
        /// <summary>
        /// The namespace where generated mocks are declared.
        /// </summary>
        public const string Namespace = "Mocks";

        /// <summary>
        /// The suffix added to mock type names.
        /// </summary>
        public const string NameSuffix = "Mock";

        /// <summary>
        /// Gets the runtime mock name from its base type and implemented interfaces.
        /// </summary>
        public static string GetName(Type baseType, Type[] implementedInterfaces)
        {
            var builder = new StringBuilder();
            AddNames(builder, new[] { baseType });
            AddNames(builder, implementedInterfaces.OrderBy(x => x.Name));
            return builder.Append(NameSuffix).ToString();
        }

        /// <summary>
        /// Gets the runtime mock full name from its base type and implemented interfaces.
        /// </summary>
        public static string GetFullName(Type baseType, Type[] implementedInterfaces)
            => Namespace + "." + GetName(baseType, implementedInterfaces);

        static void AddNames(StringBuilder builder, IEnumerable<Type> symbols)
        {
            foreach (var symbol in symbols)
            {
                if (!symbol.IsGenericType)
                {
                    builder.Append(symbol.Name);
                }
                else
                {
                    // Remove the arity of the generic type
                    builder.Append(symbol.Name.Substring(0, symbol.Name.IndexOf('`')));
                    builder.Append("Of");
                    AddNames(builder, symbol.GenericTypeArguments);
                }
            }
        }
    }
}