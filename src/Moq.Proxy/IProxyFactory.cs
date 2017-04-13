using System;

namespace Moq.Proxy
{
    /// <summary>
    /// Interface implemented by proxy factories.
    /// </summary>
	public interface IProxyFactory
	{
        /// <summary>
        /// Creates a proxy with the given parameters.
        /// </summary>
        /// <param name="baseType">The base type (or main interface) of the proxy.</param>
        /// <param name="implementedInterfaces">Optional additional interfaces to implement in the proxy.</param>
        /// <param name="construtorArguments">
        /// Optional contructor parameters if the <paramref name="baseType"/> 
        /// is a class, rather than an interface.
        /// </param>
        /// <returns>A proxy that implements <see cref="IProxy"/> in addition to the configured interfaces (if any).</returns>
		object CreateProxy(Type baseType, Type[] implementedInterfaces, object[] construtorArguments);
	}
}