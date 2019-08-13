using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Stunts.Emit.Static
{
    /// <summary>
    /// Naming conventions used for static code generation.
    /// </summary>
    internal static class StuntNaming
    {
        /// <summary>
        /// The type name to generate for the given (optional) base type and implemented interfaces.
        /// </summary>
        public static string GetName(IEnumerable<TypeReference> types)
            => GetName(Stunts.StuntNaming.DefaultSuffix, types);

        /// <summary>
        /// The type name to generate for the given (optional) base type and implemented interfaces.
        /// </summary>
        /// <dev>
        /// NOTE: this implementation *MUST* remain consistent with <see cref="Stunts.StuntNaming.GetName(string, System.Type, System.Type[])"/>
        /// </dev>
        public static string GetName(string suffix, IEnumerable<TypeReference> types)
        {
            var builder = new StringBuilder();

            // AddNames(builder, types);
            
             //First add the base class
            AddNames(builder, types.Where(x => x.Resolve().IsClass));
             //Then the interfaces
            AddNames(builder, types.Where(x => x.Resolve().IsInterface).OrderBy(x => x.Name, StringComparer.Ordinal));

            return builder.Append(suffix).ToString();
        }

        /// <summary>
        /// Gets the runtime stunt full name from its base type and implemented interfaces.
        /// </summary>
        public static string GetFullName(IEnumerable<TypeReference> types)
            => GetFullName(Stunts.StuntNaming.DefaultNamespace, Stunts.StuntNaming.DefaultSuffix, types);

        /// <summary>
        /// Gets the runtime stunt full name from its base type and implemented interfaces.
        /// </summary>
        public static string GetFullName(string @namespace, string suffix, IEnumerable<TypeReference> types)
            => @namespace + "." + GetName(suffix, types);

        /// <summary>
        /// Gets the runtime stunt type name from its base type and implemented interfaces.
        /// </summary>
        public static StuntTypeName GetTypeName(IEnumerable<TypeReference> types)
            => new StuntTypeName(Stunts.StuntNaming.DefaultNamespace, GetName(types));

        static void AddNames(this StringBuilder builder, IEnumerable<TypeReference> types)
        {
            foreach (var type in types)
            {
                builder.AddName(type);
                if (type.IsGenericInstance)
                {
                    builder.Append("Of").AddNames(((GenericInstanceType)type).GenericArguments);
                }
            }
        }

        static StringBuilder AddName(this StringBuilder builder, TypeReference type)
            => type.IsGenericInstance ? 
                builder.Append(type.Name.Substring(0, type.Name.IndexOf('`'))) : 
                builder.Append(type.Name.TrimEnd('[', ']'));
    }
}
