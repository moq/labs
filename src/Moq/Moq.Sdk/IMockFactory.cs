using System;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Interface implemented by mock factories.
    /// </summary>
	public interface IMockFactory
    {
        /// <summary>
        /// Creates a mock with the given parameters.
        /// </summary>
        /// <param name="mocksAssembly">Assembly where compile-time generated mocks exist.</param>
        /// <param name="baseType">The base type (or main interface) of the mock.</param>
        /// <param name="implementedInterfaces">Additional interfaces implemented by the mock, or an empty array.</param>
        /// <param name="construtorArguments">
        /// Constructor arguments if the <paramref name="baseType" /> is a class, rather than an interface, or an empty array.
        /// </param>
        /// <returns>A mock that implements <see cref="IMocked"/> in addition to the specified interfaces (if any).</returns>
		object CreateMock(Assembly mocksAssembly, Type baseType, Type[] implementedInterfaces, object[] construtorArguments);
    }
}
