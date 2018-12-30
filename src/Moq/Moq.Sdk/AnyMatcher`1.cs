using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using TypeNameFormatter;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches any argument with the given type <typeparamref name="T"/>, 
    /// including <see langword="null"/> if the type is a reference type 
    /// or a nullable value type.
    /// </summary>
    /// <typeparam name="T">Type of argument to match.</typeparam>
    public class AnyMatcher<T> : IArgumentMatcher, IEquatable<AnyMatcher<T>>
    {
        // Disable warning since we only use this member from this class
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

        public bool Equals(AnyMatcher<T> other) => other != null;

        public override bool Equals(object obj) => Equals(obj as AnyMatcher<T>);

        public override int GetHashCode() => typeof(T).GetHashCode();

        #endregion
    }
}
