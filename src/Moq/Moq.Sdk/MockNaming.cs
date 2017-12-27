using System;
using System.Collections.Generic;
using System.Linq;

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
            Array.Sort(implementedInterfaces, Comparer<Type>.Create((x, y) => x.Name.CompareTo(y.Name)));

            return baseType.Name + string.Join("", implementedInterfaces.Select(x => x.Name)) + NameSuffix;
        }

        /// <summary>
        /// Gets the runtime mock full name from its base type and implemented interfaces.
        /// </summary>
        public static string GetFullName(Type baseType, Type[] implementedInterfaces)
            => Namespace + "." + GetName(baseType, implementedInterfaces);
    }
}