using System.Linq;
using System.Reflection;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// A <see cref="IProxyBehavior"/> that returns default values from an 
    /// invocation, both for the method return type as well as any out/ref 
    /// parameters.
    /// </summary>
    public class DefaultValueProxyBehavior : IProxyBehavior
    {
        /// <inheritdoc />
        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            var arguments = invocation.Arguments.ToArray();
            var parameters = invocation.MethodBase.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                // This covers both out & ref
                if (parameter.ParameterType.IsByRef)
                {
                    // If type is by ref, we need to get the actual element type of the ref. 
                    // i.e. Object[]& has ElementType = Object[]
                    var parameterType = parameter.ParameterType.GetElementType();
                    arguments[i] = DefaultValue.For(parameterType);
                }
            }

            var returnValue = default(object);
            if (invocation.MethodBase is MethodInfo info &&
                info.ReturnType != typeof(void))
            {
                returnValue = DefaultValue.For(info.ReturnType);
            }

            return invocation.CreateValueReturn(returnValue, arguments);
        }
    }
}
