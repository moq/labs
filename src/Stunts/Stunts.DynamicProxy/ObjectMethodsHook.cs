using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace Stunts.Sdk
{
    /// <summary>
    /// Hook used to tells Castle which methods to proxy in mocked classes.
    /// 
    /// Here we proxy the default methods Castle suggests (everything Object's methods)
    /// plus Object.ToString(), so we can give mocks useful default names.
    /// 
    /// This is required to allow Moq to mock ToString on proxy *class* implementations.
    /// </summary>
    [Serializable]
    internal class ObjectMethodsHook : AllMethodsHook
    {
        /// <summary>
        /// Adds <see cref="object"/> methods to the interception candidates.
        /// </summary>
        public override bool ShouldInterceptMethod(Type type, MethodInfo method)
            => base.ShouldInterceptMethod(type, method) || IsObjectMethod(method);

        private static bool IsObjectMethod(MethodInfo method)
            => method.DeclaringType == typeof(object) && 
                (method.Name == nameof(object.ToString) || 
                method.Name == nameof(object.Equals) || 
                method.Name == nameof(object.GetHashCode));
    }
}
