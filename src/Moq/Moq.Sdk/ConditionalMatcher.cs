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
    public class ConditionalMatcher<T> : IArgumentMatcher, IEquatable<ConditionalMatcher<T>>
    {
        static bool IsValueType = typeof(T).GetTypeInfo().IsValueType;
        static bool IsNullable = typeof(T).GetTypeInfo().IsGenericType &&
            typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Func<T, bool> condition;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Lazy<IStructuralEquatable> equatable;

        public ConditionalMatcher(Func<T, bool> condition, string name = "condition")
        {
            this.condition = condition;
            this.name = name;
            equatable = new Lazy<IStructuralEquatable>(() => Tuple.Create(condition, name));
        }

        /// <summary>
        /// Gets the type of the argument this matcher supports.
        /// </summary>
        public Type ArgumentType => typeof(T);

        /// <summary>
        /// Evaluates whether the given value matches this instance.
        /// </summary>
        public bool Matches(object value)
        {
            // Non-nullable value types never match against a null value.
            if (IsValueType && !IsNullable && value == null)
                return false;

            return (value == null ||
                typeof(T).GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo())) &&
                condition((T)value);
        }

        public override string ToString() => "Any<" + Stringly.ToTypeName(ArgumentType) + ">(" + name + ")";

        #region Equality

        public bool Equals(ConditionalMatcher<T> other) => equatable.Value.Equals(other?.equatable?.Value, EqualityComparer<object>.Default);

        public bool Equals(object other, IEqualityComparer comparer) => equatable.Value.Equals((other as ConditionalMatcher<T>)?.equatable?.Value, comparer);

        public int GetHashCode(IEqualityComparer comparer) => equatable.Value.GetHashCode(comparer);

        public override bool Equals(object obj) => Equals(obj as ConditionalMatcher<T>);

        public override int GetHashCode() => GetHashCode(EqualityComparer<object>.Default);

        #endregion
    }
}
