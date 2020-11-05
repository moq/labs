using System;
using System.Diagnostics;
using System.Reflection;
using TypeNameFormatter;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches any argument with a given type 
    /// including <see langword="null"/> if the type is a reference type 
    /// or a nullable value type.
    /// </summary>
    public class AnyMatcher : IArgumentMatcher, IEquatable<AnyMatcher>
    {
        readonly TypeInfo info;
        readonly bool isValueType;
        readonly bool isNullable;

        /// <summary>
        /// Initializes the matcher with the given <paramref name="argumentType"/>.
        /// </summary>
        /// <param name="argumentType">Type of argument that this matcher matches.</param>
        public AnyMatcher(Type argumentType)
        {
            ArgumentType = argumentType ?? throw new ArgumentNullException();

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
        public bool Matches(object? value)
        {
            // Non-nullable value types never match against a null value.
            if (isValueType && !isNullable && value == null)
                return false;

            return value == null || info.IsAssignableFrom(value.GetType().GetTypeInfo());
        }

        /// <summary>
        /// Gets a friendly representation of the object.
        /// </summary>
        /// <devdoc>
        /// We don't want to optimize code coverage for this since it's a debugger aid only. 
        /// Annotating this method with DebuggerNonUserCode achieves that.
        /// No actual behavior depends on these strings.
        /// </devdoc>
        [DebuggerNonUserCode]
        public override string ToString() => "Any<" + ArgumentType.GetFormattedName() + ">";

        #region Equality

        /// <inheritdoc />
        public bool Equals(AnyMatcher other) => info.Equals(other.info);

        /// <inheritdoc />
        public override bool Equals(object other) => other is AnyMatcher matcher && Equals(matcher);

        /// <inheritdoc />
        public override int GetHashCode() => info.GetHashCode();

        #endregion
    }
}
