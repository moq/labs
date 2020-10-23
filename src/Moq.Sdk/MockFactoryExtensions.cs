using System;
using System.ComponentModel;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Usability functions for <see cref="IMockFactory"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MockFactoryExtensions
    {
        /// <summary>
        /// Creates a mock with the given parameters.
        /// </summary>
        /// <param name="factory">The <see cref="IMockFactory"/> used to create instances of mocks.</param>
        /// <param name="mocksAssembly">Assembly where compile-time generated mocks exist.</param>
        /// <returns>A mock that implements <see cref="IMocked"/> in addition to the specified <typeparamref name="T"/>.</returns>
        public static T CreateMock<T>(this IMockFactory factory, Assembly mocksAssembly)
            => (T)factory.CreateMock(mocksAssembly, typeof(T), new Type[0], new object[0]);
    }
}
