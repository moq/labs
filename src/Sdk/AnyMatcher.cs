using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches any argument with the given type <typeparamref name="T"/>, 
    /// including <see langword="null"/> if the type is a reference type 
    /// or a nullable value type.
    /// </summary>
    /// <typeparam name="T">Type of argument to match.</typeparam>
    public class AnyMatcher<T> : IArgumentMatcher, IEquatable<AnyMatcher<T>>, IStructuralEquatable
    {
        static bool IsValueType = typeof(T).GetTypeInfo().IsValueType;
        static bool IsNullable = typeof(T).GetTypeInfo().IsGenericType &&
            typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);

        /// <summary>
        /// Gets the singleton instance of this matcher.
        /// </summary>
        public static IArgumentMatcher Default { get; } = new AnyMatcher<T>();

        AnyMatcher() { }

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

            return value == null ||
                typeof(T).GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo());
        }

        public override string ToString() => "Any<" + Stringly.ToTypeName(ArgumentType) + ">";

        #region Equality

        public bool Equals(AnyMatcher<T> other) => Equals(other);

        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);

        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();

        #endregion
    }
}
