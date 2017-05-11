using System;
using System.Collections.Concurrent;

namespace Moq.Sdk
{
    /// <summary>
    /// A typed state bag for holding arbitrary mock state. All members are thread-safe.
    /// </summary>
    public class MockState
    {
        ConcurrentDictionary<object, object> state = new ConcurrentDictionary<object, object>();

        /// <summary>
        /// Clears the state.
        /// </summary>
        public void Clear() => state.Clear();

        /// <summary>
        /// Gets whether the state contains data of the given type <typeparamref name="T"/>.
        /// </summary>
        public bool Contains<T>() => state.ContainsKey(typeof(T));

        /// <summary>
        /// Gets whether the state contains data of the given type <typeparamref name="T"/> 
        /// and <paramref name="key"/>.
        /// </summary>
        public bool Contains<T>(string key) => state.ContainsKey(Tuple.Create(typeof(T), key));

        /// <summary>
        /// Adds the state of the given type <typeparamref name="T"/> by using the 
        /// specified function, if a value with the given type does not already exist.
        /// </summary>
        public T GetOrAdd<T>(Func<T> valueFactory)
            => (T)state.GetOrAdd(typeof(T), _ => valueFactory);

        /// <summary>
        /// Adds the state of the given type <typeparamref name="T"/> and <paramref name="key"/> 
        /// by using the specified function, if a value with the given type does not already exist.
        /// </summary>
        public T GetOrAdd<T>(string key, Func<T> valueFactory)
            => (T)state.GetOrAdd(Tuple.Create(typeof(T), key), _ => valueFactory());

        /// <summary>
        /// Attempts to add the specified value to the mock state.
        /// </summary>
        /// <typeparam name="T">The type of value to add.</typeparam>
        /// <param name="value">The value of the element to add.</param>
        /// <returns></returns>
        public bool TryAdd<T>(T value) => state.TryAdd(typeof(T), value);

        /// <summary>
        /// Attempts to add the specified value to the mock state with the given 
        /// <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to add.</typeparam>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns></returns>
        public bool TryAdd<T>(string key, T value) => state.TryAdd(Tuple.Create(typeof(T), key), value);

        /// <summary>
        /// Attempts to get the value of the given <typeparamref name="T"/> from the mock state.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="value">When this method returns, contains the object from the mock state
        /// that has the specified <typeparamref name="T"/>, or the default value of the type if 
        /// the operation failed.</param>
        public bool TryGetValue<T>(out T value)
        {
            var result = state.TryGetValue(typeof(T), out var _value);
            value = (T)_value;
            return result;
        }

        /// <summary>
        /// Attempts to get the value of the given <typeparamref name="T"/> with the 
        /// given <paramref name="key"/> from the mock state.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the object from the mock state
        /// that has the specified <typeparamref name="T"/>, or the default value of the type if 
        /// the operation failed.</param>
        public bool TryGetValue<T>(string key, out T value)
        {
            var result = state.TryGetValue(Tuple.Create(typeof(T), key), out var _value);
            value = (T)_value;
            return result;
        }

        /// <summary>
        /// Attempts to remove and return the value of the given type 
        /// <typeparamref name="T"/> from the mock state.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="value">When this method returns, contains the object removed from the 
        /// mock state, or the default value of the <typeparamref name="T"/> type if key does not exist.</param>
        /// <returns><see langword="true"/> if the object was removed successfully; otherwise, <see langword="false"/>.</returns>
        public bool TryRemove<T>(out T value)
        {
            var result = state.TryRemove(typeof(T), out var _value);
            value = (T)_value;
            return result;
        }

        /// <summary>
        /// Attempts to remove and return the value of the given type 
        /// <typeparamref name="T"/> and <paramref name="key"/> from the mock state.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, contains the object removed from the 
        /// mock state, or the default value of the <typeparamref name="T"/> type if key does not exist.</param>
        /// <returns><see langword="true"/> if the object was removed successfully; otherwise, <see langword="false"/>.</returns>
        public bool TryRemove<T>(string key, out T value)
        {
            var result = state.TryRemove(Tuple.Create(typeof(T), key), out var _value);
            value = (T)_value;
            return result;
        }
        
        /// <summary>
        /// Compares the existing value for of the specified <typeparamref name="T"/> with a specified value, 
        /// and if they are equal, updates it with a third value.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="newValue">The value that replaces the value of the element that has the specified 
        /// key if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element that has 
        /// the specified <typeparamref name="T"/>.</param>
        /// <returns><see langword="true"/> if the value with the given <typeparamref name="T"/> was equal 
        /// to <paramref name="comparisonValue"/> and was replaced with <paramref name="newValue"/>; otherwise, 
        /// <see langword="false"/>.</returns>
        public bool TryUpdate<T>(T newValue, T comparisonValue) => state.TryUpdate(typeof(T), newValue, comparisonValue);

        /// <summary>
        /// Compares the existing value for of the specified <typeparamref name="T"/> with a specified value, 
        /// and if they are equal, updates it with a third value.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="key">The key whose value is compared with <paramref name="comparisonValue"/> and possibly replaced.</param>
        /// <param name="newValue">The value that replaces the value of the element that has the specified 
        /// key if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element that has 
        /// the specified <typeparamref name="T"/>.</param>
        /// <returns><see langword="true"/> if the value with the given <typeparamref name="T"/> was equal 
        /// to <paramref name="comparisonValue"/> and was replaced with <paramref name="newValue"/>; otherwise, 
        /// <see langword="false"/>.</returns>
        public bool TryUpdate<T>(string key, T newValue, T comparisonValue) => state.TryUpdate(Tuple.Create(typeof(T), key), newValue, comparisonValue);
    }
}