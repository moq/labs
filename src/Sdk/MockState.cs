using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace Moq.Sdk
{
    /// <summary>
    /// A typed state bag for holding arbitrary mock state. All members are thread-safe.
    /// </summary>
    public class MockState
    {
        ConcurrentDictionary<object, object> state = new ConcurrentDictionary<object, object>();

        public bool Contains<T>() => state.ContainsKey(typeof(T));

        public bool Contains<T>(string key) => state.ContainsKey(Tuple.Create(typeof(T), key));

        public T GetOrAdd<T>(Func<T> valueFactory)
            => (T)state.GetOrAdd(typeof(T), _ => valueFactory);

        public T GetOrAdd<T>(string key, Func<T> valueFactory)
            => (T)state.GetOrAdd(Tuple.Create(typeof(T), key), _ => valueFactory());

        public bool TryAdd<T>(T value) => state.TryAdd(typeof(T), value);

        public bool TryAdd<T>(string key, T value) => state.TryAdd(Tuple.Create(typeof(T), key), value);

        public bool TryGetValue<T>(out T value)
        {
            var result = state.TryGetValue(typeof(T), out var _value);
            value = (T)_value;
            return result;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var result = state.TryGetValue(Tuple.Create(typeof(T), key), out var _value);
            value = (T)_value;
            return result;
        }

        public bool TryRemove<T>(out T value)
        {
            var result = state.TryRemove(typeof(T), out var _value);
            value = (T)_value;
            return result;
        }
        public bool TryRemove<T>(string key, out T value)
        {
            var result = state.TryRemove(Tuple.Create(typeof(T), key), out var _value);
            value = (T)_value;
            return result;
        }

        public bool TryUpdate<T>(T newValue, T comparisonValue) => state.TryUpdate(typeof(T), newValue, comparisonValue);

        public bool TryUpdate<T>(string key, T newValue, T comparisonValue) => state.TryUpdate(Tuple.Create(typeof(T), key), newValue, comparisonValue);
    }
}
