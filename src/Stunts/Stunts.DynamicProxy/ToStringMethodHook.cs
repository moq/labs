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
    internal class ToStringMethodHook : AllMethodsHook
    {
        /// <summary>
        /// Extends AllMethodsHook.ShouldInterceptMethod to also intercept Object.ToString().
        /// </summary>
        public override bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            var isObjectToString = methodInfo.DeclaringType == typeof(object) && methodInfo.Name == "ToString";

            return base.ShouldInterceptMethod(type, methodInfo) || isObjectToString;
        }
    }
}
