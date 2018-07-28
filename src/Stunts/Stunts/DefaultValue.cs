using System;
using System.Collections;
using System.Collections.Concurrent;
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
    public class DefaultValue
    {
        ConcurrentDictionary<Type, Func<Type, object>> factories = new ConcurrentDictionary<Type, Func<Type, object>>();

        public DefaultValue(bool addDefaults = true)
        {
            if (addDefaults)
            {
                factories[typeof(Array)] = CreateArray;
                factories[typeof(Task)] = CreateTask;
                factories[typeof(Task<>)] = CreateTaskOf;
                factories[typeof(IEnumerable)] = CreateEnumerable;
                factories[typeof(IEnumerable<>)] = CreateEnumerableOf;
            }
        }
        
        /// <summary>
        /// Calculates the default value for the given type <typeparamref name="T"/>.
        /// </summary>
        public T For<T>() => (T)For(typeof(T));

        /// <summary>
        /// Calculates the default value for the given type <paramref name="type"/>
        /// </summary>
        public object For(Type type)
        {
            var valueType = type.IsByRef && type.HasElementType ? type.GetElementType() : type;
            var info = valueType.GetTypeInfo();

            // If type is by ref, we need to get the actual element type of the ref. 
            // i.e. Object[]& has ElementType = Object[]
            var typeKey = valueType.IsArray ? typeof(Array) : valueType;

            // Try get a handler with the concrete type first.
            if (factories.TryGetValue(typeKey, out var factory))
                return factory.Invoke(typeKey);

            // Fallback to getting one for the generic type, if available
            if (info.IsGenericType && factories.TryGetValue(valueType.GetGenericTypeDefinition(), out factory))
                return factory.Invoke(valueType);

            return GetFallbackDefaultValue(valueType);
        }

        public bool Deregister(Type key) => factories.TryRemove(key, out _);

        public void Register(Type key, Func<Type, object> factory) => factories[key] = factory;

        object CreateArray(Type type) => Array.CreateInstance(type, 0);

        object CreateTask(Type type) => Task.CompletedTask;

        object CreateTaskOf(Type type) => GetCompletedTaskForType(type.GetTypeInfo().GenericTypeArguments[0]);

        object CreateEnumerable(Type type) => Enumerable.Empty<object>();

        object CreateEnumerableOf(Type type) => Array.CreateInstance(type.GetTypeInfo().GenericTypeArguments[0], 0);

        object GetFallbackDefaultValue(Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                // For nullable value types, return null.
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return null;

                return Activator.CreateInstance(type);
            }

            return null;
        }

        Task GetCompletedTaskForType(Type type)
        {
            var tcs = Activator.CreateInstance(typeof(TaskCompletionSource<>).MakeGenericType(type));

            var setResultMethod = tcs.GetType().GetTypeInfo().GetDeclaredMethod("SetResult");
            var taskProperty = tcs.GetType().GetTypeInfo().GetDeclaredProperty("Task");
            var result = For(type);

            setResultMethod.Invoke(tcs, new[] { result });

            return (Task)taskProperty.GetValue(tcs, null);
        }
    }
}
