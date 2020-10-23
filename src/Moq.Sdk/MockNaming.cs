using System;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Exposes the naming conventions for mock generation and instantiation.
    /// </summary>
    public static class MockNaming
    {
        /// <summary>
        /// The default namespace where generated mocks are declared.
        /// </summary>
        public const string DefaultNamespace = "Mocks";

        /// <summary>
        /// The default suffix added to mock type names.
        /// </summary>
        public const string DefaultSuffix = "Mock";

        /// <summary>
        /// Gets the runtime mock name from its base type and optional additional 
        /// interfaces, using the <see cref="DefaultSuffix"/>.
        /// </summary>
        public static string GetName(Type baseType, Type[] additionalInterfaces)
            => GetName(DefaultSuffix, baseType, additionalInterfaces);

        /// <summary>
        /// Gets the runtime mock name from its base type and optional additional interfaces 
        /// and the given <paramref name="suffix"/> appended to the type name.
        /// </summary>
        public static string GetName(string suffix, Type baseType, Type[] additionalInterfaces)
            => StuntNaming.GetName(suffix, baseType, additionalInterfaces);

        /// <summary>
        /// Gets the runtime mock full name from its base type and optional additional interfaces,
        /// using the <see cref="DefaultNamespace"/> and <see cref="DefaultSuffix"/>.
        /// </summary>
        public static string GetFullName(Type baseType, params Type[] additionalInterfaces)
            => GetFullName(DefaultNamespace, DefaultSuffix, baseType, additionalInterfaces);

        /// <summary>
        /// Gets the runtime mock full name from its base type and implemented interfaces.
        /// </summary>
        public static string GetFullName(string @namespace, Type baseType, params Type[] additionalInterfaces)
            => GetFullName(@namespace, DefaultSuffix, baseType, additionalInterfaces);

        /// <summary>
        /// Gets the runtime mock full name from its base type and implemented interfaces.
        /// </summary>
        public static string GetFullName(string @namespace, string suffix, Type baseType, params Type[] additionalInterfaces)
            => StuntNaming.GetFullName(@namespace, suffix, baseType, additionalInterfaces);
    }
}