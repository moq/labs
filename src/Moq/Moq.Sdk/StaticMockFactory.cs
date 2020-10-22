using System;
using System.ComponentModel;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Provides a <see cref="IMockFactory"/> that creates mocks from types 
    /// generated at compile-time that are included in the received mock  
    /// assembly in <see cref="CreateMock"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StaticMockFactory : IMockFactory
    {
        /// <summary>
        /// Uses the <see cref="MockNaming.GetFullName(Type, Type[])"/> method to 
        /// determine the expected full type name of a compile-time generated mocks 
        /// and tries to locate it from <paramref name="mocksAssembly"/>.
        /// </summary>
        /// <param name="mocksAssembly">The assembly containing the compile-time generated mocks.</param>
        /// <param name="baseType">Base type of the mock.</param>
        /// <param name="implementedInterfaces">Additional interfaces the mock implements.</param>
        /// <param name="constructorArguments">Optional additional constructor arguments for the mock.</param>
        public object CreateMock(Assembly mocksAssembly, Type baseType, Type[] implementedInterfaces, object[] constructorArguments)
        {
            var name = MockNaming.GetFullName(baseType, implementedInterfaces);
            var type = mocksAssembly.GetType(name, true, false);

            return Activator.CreateInstance(type, constructorArguments);
        }
    }
}
