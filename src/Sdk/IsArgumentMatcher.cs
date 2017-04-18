using System;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches any argument with the given type <typeparamref name="T"/>, 
    /// including <see langword="null"/> if the type is a reference type 
    /// or a nullable value type.
    /// </summary>
    /// <typeparam name="T">Type of argument to match.</typeparam>
    public class IsArgumentMatcher<T> : IArgumentMatcher
    {
        static bool IsValueType = typeof(T).GetTypeInfo().IsValueType;
        static bool IsNullable = typeof(T).GetTypeInfo().IsGenericType &&
            typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);

        Func<T, bool> condition;

        public IsArgumentMatcher(Func<T, bool> condition) => this.condition = condition;

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

            return value != null ||
                typeof(T).GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()) &&
                condition((T)value);
        }

        public override bool Equals(object obj) => Equals(this, obj as IsArgumentMatcher<T>);

        public override int GetHashCode() => condition.GetHashCode();

        static bool Equals(IsArgumentMatcher<T> obj1, IsArgumentMatcher<T> obj2) => ReferenceEquals(obj1.condition, obj2.condition);
    }
}
