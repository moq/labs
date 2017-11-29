using System;
using System.Reflection;

namespace Stunts
{
    /// <summary>
    /// Interface implemented by stunt factories.
    /// </summary>
	public interface IStuntFactory
	{
        /// <summary>
        /// Creates a stunt with the given parameters.
        /// </summary>
        /// <param name="stuntsAssembly">Assembly where compile-time generated stunts exist.</param>
        /// <param name="baseType">The base type (or main interface) of the stunt.</param>
        /// <param name="implementedInterfaces">Additional interfaces implemented by the stunt, or an empty array.</param>
        /// <param name="construtorArguments">
        /// Contructor arguments if the <paramref name="baseType" /> is a class, rather than an interface, or an empty array.
        /// </param>
        /// <returns>A stunt that implements <see cref="IStunt"/> in addition to the specified interfaces (if any).</returns>
		object CreateStunt(Assembly stuntsAssembly, Type baseType, Type[] implementedInterfaces, object[] construtorArguments);
	}
}