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
    public class AnyValueMatcher<T> : IArgumentMatcher
    {
        static bool IsValueType = typeof(T).GetTypeInfo().IsValueType;
        static bool IsNullable = typeof(T).GetTypeInfo().IsGenericType &&
            typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);

        /// <summary>
        /// Gets the singleton instance of this matcher.
        /// </summary>
        public static IArgumentMatcher Default { get; } = new AnyValueMatcher<T>();

        AnyValueMatcher() { }

        /// <summary>
        /// Gets the type of the argument this matcher supports.
        /// </summary>
        public Type ArgumentType { get { return typeof(T); } }

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

        public override bool Equals(object obj) => Equals(this, obj as AnyValueMatcher<T>);

        public override int GetHashCode() => typeof(T).GetHashCode();

        static bool Equals(AnyValueMatcher<T> obj1, AnyValueMatcher<T> obj2)
        {
            if (object.Equals(null, obj1) ||
                object.Equals(null, obj2) ||
                obj1.GetType() != obj2.GetType())
                return false;

            // If both are non-null and they have the same type, 
            // they match essentially any value of the same type, 
            // so they are equal.
            return true;
        }
    }
}
