using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Stunts
{
    /// <summary>
    /// Utility class that generates default values for certain types.
    /// See <see cref="DefaultValueBehavior"/>.
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
        public static object For(Type type)
        {
            // If type is by ref, we need to get the actual element type of the ref. 
            // i.e. Object[]& has ElementType = Object[]
            var actualType = type.IsByRef && type.HasElementType ?
                type.GetElementType() : type;

            return actualType.GetTypeInfo().IsValueType ?
                GetDefaultValueType(actualType) :
                GetDefaultReferenceType(actualType);
        }

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
