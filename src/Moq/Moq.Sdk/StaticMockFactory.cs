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
        /// <inheritdoc />
        public object CreateMock(Assembly mocksAssembly, Type baseType, Type[] implementedInterfaces, object[] constructorArguments)
            => throw new NotImplementedException();
    }
}
