using System;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches an argument with the given type <typeparamref name="T"/>, 
    /// as long as it satisfies a given condition.
    /// </summary>
    /// <typeparam name="T">Type of argument being conditioned.</typeparam>
    public class ConditionalMatcher<T> : IArgumentMatcher
    {
        static bool IsValueType = typeof(T).GetTypeInfo().IsValueType;
        static bool IsNullable = typeof(T).GetTypeInfo().IsGenericType &&
            typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);

        Func<T, bool> condition;

        public ConditionalMatcher(Func<T, bool> condition) => this.condition = condition;

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

        public override bool Equals(object obj) => Equals(this, obj as ConditionalMatcher<T>);

        public override int GetHashCode() => condition.GetHashCode();

        static bool Equals(ConditionalMatcher<T> obj1, ConditionalMatcher<T> obj2) => ReferenceEquals(obj1.condition, obj2.condition);

        public override string ToString() => "Any<" + Stringly.ToTypeName(ArgumentType) + ">(condition)";
    }
}
