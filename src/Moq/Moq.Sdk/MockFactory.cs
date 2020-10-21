using System;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Allows accessing the default <see cref="IMockFactory"/> used 
    /// to create mocks.
    /// </summary>
    public class MockFactory : IMockFactory
    {
        static readonly IMockFactory nullFactory = new MockFactory();

        /// <summary>
        /// Gets or sets the default <see cref="IMockFactory"/> to use 
        /// to create mocks.  Defaults to the <see cref="NotImplemented"/> factory.
        /// </summary>
        public static IMockFactory Default { get; set; } = nullFactory;

        /// <summary>
        /// A factory that throws <see cref="NotImplementedException"/>.
        /// </summary>
        public static IMockFactory NotImplemented { get; } = nullFactory;

        private MockFactory() { }

        /// <summary>
        /// Throws <see cref="NotImplementedException"/> since this is the default 
        /// <see cref="NotImplemented"/> factory.
        /// </summary>
        public object CreateMock(Assembly mocksAssembly, Type baseType, Type[] implementedInterfaces, object[] constructorArguments)
            => throw new NotImplementedException(ThisAssembly.Strings.MockFactoryNotImplemented);
    }
}