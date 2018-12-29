using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Moq.Sdk
{
    /// <summary>
    /// Utility class that generates default values for certain types.
    /// See <see cref="DefaultValueBehavior"/>.
    /// </summary>
    public class DefaultValueProvider
    {
        readonly ConcurrentDictionary<Type, Func<Type, object>> factories = new ConcurrentDictionary<Type, Func<Type, object>>();

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="registerDefaults">Whether to register the default value factories.</param>
        public DefaultValueProvider(bool registerDefaults = true)
        {
            if (registerDefaults)
            {
                factories[typeof(Array)] = CreateArray;
                factories[typeof(Task)] = CreateTask;
                factories[typeof(Task<>)] = CreateTaskOf;
                factories[typeof(IEnumerable)] = CreateEnumerable;
                factories[typeof(IEnumerable<>)] = CreateEnumerableOf;
                factories[typeof(IQueryable)] = CreateQueryable;
                factories[typeof(IQueryable<>)] = CreateQueryableOf;
                factories[typeof(ValueTuple<>)] = CreateValueTupleOf;
				factories[typeof(ValueTuple<,>)] = CreateValueTupleOf;
				factories[typeof(ValueTuple<,,>)] = CreateValueTupleOf;
				factories[typeof(ValueTuple<,,,>)] = CreateValueTupleOf;
				factories[typeof(ValueTuple<,,,,>)] = CreateValueTupleOf;
				factories[typeof(ValueTuple<,,,,,>)] = CreateValueTupleOf;
				factories[typeof(ValueTuple<,,,,,,>)] = CreateValueTupleOf;
				factories[typeof(ValueTuple<,,,,,,,>)] = CreateValueTupleOf;
            }
        }
        
        /// <summary>
        /// Gets a default value for the given type <typeparamref name="T"/>.
        /// </summary>
        public T For<T>() => (T)For(typeof(T));

        /// <summary>
        /// Gets a default value for the given type <paramref name="type"/>
        /// </summary>
        public object For(Type type)
        {
            // If type is by ref, we need to get the actual element type of the ref. 
            // i.e. Object[]& has ElementType = Object[]
            var valueType = type.IsByRef && type.HasElementType ? type.GetElementType() : type;
            var info = valueType.GetTypeInfo();
            var typeKey = valueType.IsArray ? typeof(Array) : valueType;

            // Try get a handler with the concrete type first.
            if (factories.TryGetValue(typeKey, out var factory))
                return factory.Invoke(valueType);

            // Fallback to getting one for the generic type, if available
            if (info.IsGenericType && factories.TryGetValue(valueType.GetGenericTypeDefinition(), out factory))
                return factory.Invoke(valueType);

            return GetFallbackDefaultValue(valueType);
        }

        /// <summary>
        /// Deregisters a default value factory for the given <paramref name="key"/>.
        /// </summary>
        /// <returns>Whether there was a registered factory and it was removed.</returns>
        public bool Deregister(Type key) => factories.TryRemove(key, out _);

        /// <summary>
        /// Registers a default value factory for the given <paramref name="key"/>.
        /// </summary>
        public void Register(Type key, Func<Type, object> factory) => factories[key] = factory;

        /// <summary>
        /// Deregisters a default value factory for the given <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Whether there was a registered factory and it was removed.</returns>
        public bool Deregister<T>() => Deregister(typeof(T));

        /// <summary>
        /// Registers a default value factory for the given <typeparamref name="T"/>.
        /// </summary>
        public void Register<T>(Func<T> factory) => Register(typeof(T), _ => factory());

        /// <summary>
        /// Determines the default value for the given <paramref name="type"/> when no suitable factory is registered for it.
        /// </summary>
        /// <param name="type">The type of which to produce a value.</param>
        protected virtual object GetFallbackDefaultValue(Type type)
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

        private static object CreateArray(Type type) => Array.CreateInstance(type.GetElementType(), new int[type.GetArrayRank()]);

        private static object CreateTask(Type type) => Task.CompletedTask;

        private static object CreateEnumerable(Type type) => Enumerable.Empty<object>();

        private static object CreateEnumerableOf(Type type) => Array.CreateInstance(type.GetTypeInfo().GenericTypeArguments[0], 0);

        private static object CreateQueryable(Type type) => Enumerable.Empty<object>().AsQueryable();

        private static object CreateQueryableOf(Type type)
        {
            var elementType = type.GetGenericArguments()[0];
            var array = Array.CreateInstance(elementType, 0);

            return typeof(Queryable).GetMethods()
                .Single(x => x.Name == nameof(Queryable.AsQueryable) && x.IsGenericMethod)
                .MakeGenericMethod(elementType)
                .Invoke(null, new[] { array });
        }

        private object CreateValueTupleOf(Type type)
        {
            var itemTypes = type.GetGenericArguments();
            var items = new object[itemTypes.Length];
            for (int i = 0, n = itemTypes.Length; i < n; ++i)
            {
                items[i] = For(itemTypes[i]);
            }
            
            return Activator.CreateInstance(type, items);

        }
        private object CreateTaskOf(Type type) => GetCompletedTaskForType(type.GetTypeInfo().GenericTypeArguments[0]);

        private Task GetCompletedTaskForType(Type type)
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
