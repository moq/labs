using System;
using System.Collections;
using System.Reflection;
using TypeNameFormatter;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches any argument with a given type 
    /// including <see langword="null"/> if the type is a reference type 
    /// or a nullable value type.
    /// </summary>
    public class AnyMatcher : IArgumentMatcher, IEquatable<AnyMatcher>, IStructuralEquatable
    {
        TypeInfo info;
        bool isValueType;
        bool isNullable;

        public AnyMatcher(Type argumentType)
        {
            ArgumentType = argumentType;

            info = argumentType.GetValueTypeInfo();
            isValueType = info.IsValueType;
            isNullable = isValueType && info.IsGenericType &&
                argumentType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Gets the type of the argument this matcher supports.
        /// </summary>
        public Type ArgumentType { get; }

        /// <summary>
        /// Evaluates whether the given value matches this instance.
        /// </summary>
        public bool Matches(object value)
        {
            // Non-nullable value types never match against a null value.
            if (isValueType && !isNullable && value == null)
                return false;

            return value == null || info.IsAssignableFrom(value.GetType().GetTypeInfo());
        }

        public override string ToString() => "Any<" + ArgumentType.GetFormattedName() + ">";

        #region Equality

        public bool Equals(AnyMatcher other) => Equals(other);

        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);

        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();

        #endregion
    }
}
