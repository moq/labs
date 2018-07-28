using System;
using System.ComponentModel;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Usability functions for <see cref="IMockFactory"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class MockFactoryExtensions
    {
        /// <summary>
        /// Creates a mock with the given parameters.
        /// </summary>
        /// <param name="mocksAssembly">Assembly where compile-time generated mocks exist.</param>
        /// <param name="baseType">The base type (or main interface) of the mock.</param>
        /// <returns>A mock that implements <see cref="IMocked"/> in addition to the specified interfaces (if any).</returns>
		public static T CreateMock<T>(this IMockFactory factory, Assembly mocksAssembly)
            => (T)factory.CreateMock(mocksAssembly, typeof(T), new Type[0], new object[0]);
    }
}
