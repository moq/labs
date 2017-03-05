using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// A <see cref="IProxyBehavior"/> that returns default values from an 
    /// invocation, both for the method return type as well as any out/ref 
    /// parameters.
    /// </summary>
    public class DefaultValueBehavior : IProxyBehavior
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
                    arguments[i] = parameterType.GetTypeInfo().IsValueType ?
                        GetValueTypeDefault(parameterType) :
                        GetReferenceTypeDefault(parameterType);
                }
            }

            var returnValue = default(object);
            if (invocation.MethodBase is MethodInfo info &&
                info.ReturnType != typeof(void))
            {
                returnValue = info.ReturnType.GetTypeInfo().IsValueType ?
                    GetValueTypeDefault(info.ReturnType) :
                    GetReferenceTypeDefault(info.ReturnType);
            }

            return invocation.CreateValueReturn(returnValue, arguments);
        }

        static object GetReferenceTypeDefault(Type valueType)
        {
            if (valueType.IsArray)
            {
                return Array.CreateInstance(valueType, 0);
            }
            else if (valueType == typeof(Task))
            {
                return Task.CompletedTask;
            }
            else if (valueType == typeof(IEnumerable))
            {
                return Enumerable.Empty<object>();
            }
            else if (valueType.GetTypeInfo().IsGenericType && valueType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var genericListType = typeof(List<>).MakeGenericType(valueType.GetTypeInfo().GenericTypeArguments[0]);
                return Activator.CreateInstance(genericListType);
            }
            else if (valueType.GetTypeInfo().IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var genericType = valueType.GetTypeInfo().GenericTypeArguments[0];
                return GetCompletedTaskForType(genericType);
            }

            return null;
        }

        static object GetValueTypeDefault(Type valueType)
        {
            // For nullable value types, return null.
            if (valueType.GetTypeInfo().IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return null;

            return Activator.CreateInstance(valueType);
        }

        static Task GetCompletedTaskForType(Type type)
        {
            var tcs = Activator.CreateInstance(typeof(TaskCompletionSource<>).MakeGenericType(type));

            var setResultMethod = tcs.GetType().GetTypeInfo().GetDeclaredMethod("SetResult");
            var taskProperty = tcs.GetType().GetTypeInfo().GetDeclaredProperty("Task");
            var result = type.GetTypeInfo().IsValueType ? GetValueTypeDefault(type) : GetReferenceTypeDefault(type);
            
            setResultMethod.Invoke(tcs, new[] { result });

            return (Task)taskProperty.GetValue(tcs, null);
        }
    }
}
