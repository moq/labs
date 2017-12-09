using System;
using System.Collections.Generic;
using System.Linq;

namespace Stunts
{
    /// <summary>
    /// Exposes the naming conventions for stunt generation and instantiation.
    /// </summary>
    public static class StuntNaming
    {
        /// <summary>
        /// The namespace where generated stunts are declared.
        /// </summary>
        public const string Namespace = "Stunts";

        /// <summary>
        /// The suffix added to stunt type names.
        /// </summary>
        public const string NameSuffix = "Stunt";

        /// <summary>
        /// Gets the runtime stunt name from its base type and implemented interfaces.
        /// </summary>
        public static string GetName(Type baseType, Type[] implementedInterfaces)
        {
            Array.Sort(implementedInterfaces, Comparer<Type>.Create((x, y) => x.Name.CompareTo(y.Name)));

            return baseType.Name + string.Join("", implementedInterfaces.Select(x => x.Name)) + NameSuffix;
        }

        /// <summary>
        /// Gets the runtime stunt full name from its base type and implemented interfaces.
        /// </summary>
        public static string GetFullName(Type baseType, Type[] implementedInterfaces)
            => Namespace + "." + GetName(baseType, implementedInterfaces);
    }
}
