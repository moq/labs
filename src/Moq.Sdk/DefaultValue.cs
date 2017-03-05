using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Moq.Sdk
{
    /// <summary>
    /// Utility class that generates default values for certain types.
    /// See <see cref="DefaultValueProxyBehavior"/>.
    /// </summary>
    public static class DefaultValue
    {
        /// <summary>
        /// Calculates the default value for the given type <typeparamref name="T"/>.
        /// </summary>
        public static T For<T>() => (T)For(typeof(T));

        /// <summary>
        /// Calculates the default value for the given type <paramref name="type"/>
        /// </summary>
        public static object For(Type type) => type.GetTypeInfo().IsValueType ?
            GetDefaultValueType(type) : 
            GetDefaultReferenceType(type);

        static object GetDefaultReferenceType(Type valueType)
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

        static object GetDefaultValueType(Type valueType)
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
            var result = type.GetTypeInfo().IsValueType ? GetDefaultValueType(type) : GetDefaultReferenceType(type);

            setResultMethod.Invoke(tcs, new[] { result });

            return (Task)taskProperty.GetValue(tcs, null);
        }
    }
}
