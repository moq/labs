using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches an argument with the given type <typeparamref name="T"/>, 
    /// as long as it satisfies a given condition.
    /// </summary>
    /// <typeparam name="T">Type of argument being conditioned.</typeparam>
    public class NotMatcher<T> : IArgumentMatcher, IEquatable<NotMatcher<T>>
    {
        static bool IsValueType = typeof(T).GetTypeInfo().IsValueType;
        static bool IsNullable = typeof(T).GetTypeInfo().IsGenericType &&
            typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        T value;

        public NotMatcher(T value) => this.value = value;

        /// <summary>
        /// Gets the type of the argument this matcher supports.
        /// </summary>
        public Type ArgumentType => typeof(T);

        /// <summary>
        /// Evaluates whether the given value matches this instance.
        /// </summary>
        public bool Matches(object value)
        {
            return !(Object.Equals(this.value, value));
        }

        public override string ToString() => "!" + (value is string ? "\"" + value + "\"" : value.ToString());

        #region Equality

        public bool Equals(NotMatcher<T> other) => other == null ? false : EqualityComparer<object>.Default.Equals(value, other.value);

        public bool Equals(object other, IEqualityComparer comparer) => comparer.Equals(this, other as NotMatcher<T>);

        public int GetHashCode(IEqualityComparer comparer) => comparer.GetHashCode(value);

        public override bool Equals(object obj) => Equals(obj as NotMatcher<T>);

        public override int GetHashCode() => GetHashCode(EqualityComparer<object>.Default);

        #endregion
    }
}
