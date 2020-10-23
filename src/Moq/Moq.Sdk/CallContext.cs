﻿using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Moq.Sdk
{
    /// <summary>
    /// Provides a way to set contextual data that flows with the call and 
    /// async context of a test or invocation.
    /// </summary>
    public static class CallContext<T>
    {
        private static readonly string defaultName = typeof(T).FullName;
        private static readonly ConcurrentDictionary<string, AsyncLocal<T?>> state = new();

        /// <summary>
        /// Stores a given object.
        /// </summary>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(T? data) => SetData(defaultName, data);

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, T? data) 
            => state.GetOrAdd(name, _ => new AsyncLocal<T?>()).Value = data;

        /// <summary>
        /// Retrieves an object from the <see cref="CallContext"/>.
        /// </summary>
        /// <returns>The object in the call context associated with the specified name, or a default value for <typeparamref name="T"/> if none is found.</returns>
        public static T? GetData() => GetData(defaultName);

        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext"/>.
        /// </summary>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or a default value for <typeparamref name="T"/> if none is found.</returns>
        public static T? GetData(string name) 
            => state.TryGetValue(name, out var data) ? data.Value : default;

        /// <summary>
        /// Retrieves an object from the <see cref="CallContext"/>, and 
        /// sets an initial value if it was not found.
        /// </summary>
        /// <param name="setInitialValue">A function that will set the initial value of the given parameter, if it doesn't have a value already in the context.</param>
        /// <returns>The object in the call context associated with the specified name, or the initial value returned from <paramref name="setInitialValue"/>.</returns>
        public static T? GetData(Func<T?> setInitialValue) => GetData(defaultName, setInitialValue);

        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext"/>, and 
        /// sets an initial value if it was not found.
        /// </summary>
        /// <param name="name">The name of the item in the call context.</param>
        /// <param name="setInitialValue">A function that will set the initial value of the given parameter, if it doesn't have a value already in the context.</param>
        /// <returns>The object in the call context associated with the specified name, or the initial value returned from <paramref name="setInitialValue"/>.</returns>
        public static T? GetData(string name, Func<T?> setInitialValue)
        {
            var local = state.GetOrAdd(name, _ => new AsyncLocal<T?> { Value = setInitialValue() });
            if (Equals(local.Value, default(T)))
                local.Value = setInitialValue();

            return local.Value;
        }
    }

    /// <summary>
    /// Provides a way to set contextual data that flows with the call and 
    /// async context of a test or invocation.
    /// </summary>
    public static class CallContext
    {
        private static readonly ConcurrentDictionary<string, AsyncLocal<object?>> state = new();

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, object? data) 
            => state.GetOrAdd(name, _ => new AsyncLocal<object?>()).Value = data;

        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext"/>.
        /// </summary>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or <see langword="null"/> if not found.</returns>
        public static object? GetData(string name) 
            => state.TryGetValue(name, out var data) ? data.Value : null;

        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> with a message stating that an 
        /// unexpected null value was found in the context.
        /// </summary>
        /// <typeparam name="T">Type of value that was unexpectedly null in the context. This is used as the key.</typeparam>
        public static T ThrowUnexpectedNull<T>()
            => throw new InvalidOperationException(ThisAssembly.Strings.UnexpectedNullContextState(typeof(T).FullName));
    }
}
