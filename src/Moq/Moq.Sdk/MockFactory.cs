using System;
using System.Reflection;
using Stunts;

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

        private MockFactory() { }

        /// <summary>
        /// See <see cref="IMockFactory.CreateMock(Assembly, Type, Type[], object[])"/>
        /// </summary>
        public object CreateMock(Assembly mocksAssembly, Type baseType, Type[] implementedInterfaces, object[] construtorArguments)
        {
            var name = MockNaming.GetFullName(baseType, implementedInterfaces);
            var type = mocksAssembly.GetType(name, true, false);

            return  (IMocked)Activator.CreateInstance(type, construtorArguments);
        }
    }
}