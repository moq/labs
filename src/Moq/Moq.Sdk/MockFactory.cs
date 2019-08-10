using System;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Allows accessing the default <see cref="IMockFactory"/> to use to 
    /// create mocks.
    /// </summary>
    public class MockFactory : IMockFactory
    {
        /// <summary>
        /// Gets or sets the default <see cref="IMockFactory"/> to use 
        /// to create mocks.
        /// </summary>
        public static IMockFactory Default { get; set; } = new MockFactory();

        /// <summary>
        /// Initializes a new <see cref="IMockFactory"/>.
        /// </summary>
        public MockFactory() { }

        /// <summary>
        /// See <see cref="IMockFactory.CreateMock(Assembly, Type, Type[], object[])"/>
        /// </summary>
        public object CreateMock(Assembly mocksAssembly, Type baseType, Type[] implementedInterfaces, object[] constructorArguments)
        {
            var type = GetMockType(mocksAssembly, baseType, implementedInterfaces, constructorArguments);
            var mocked = CreateMock(type, constructorArguments);

            // Save for cloning purposes.
            mocked.Mock.State.Set(".ctor", constructorArguments);

            return mocked;
        }

        /// <summary>
        /// Creates an instance of the mock, which must implement <see cref="IMocked"/> interface.
        /// </summary>
        protected IMocked CreateMock(Type type, object[] constructorArguments)
            => (IMocked)Activator.CreateInstance(type, constructorArguments);

        /// <summary>
        /// Default implementation for locating mock types will use <see cref="MockNaming.GetFullName(Type, Type[])"/> to 
        /// create a candidate mock type name, then get it from the <paramref name="mocksAssembly"/>.
        /// </summary>
        protected virtual Type GetMockType(Assembly mocksAssembly, Type baseType, Type[] implementedInterfaces, object[] constructorArguments)
        {
            var name = MockNaming.GetFullName(baseType, implementedInterfaces);
            return mocksAssembly.GetType(name, true, false);
        }
    }
}