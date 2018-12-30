using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Stunts;
using TypeNameFormatter;

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

        public ConditionalMatcher(Func<T, bool> condition, string name = "condition")
        {
            this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
            this.name = name ?? throw new ArgumentNullException(nameof(name));
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
                typeof(T) == value.GetType() ||
                typeof(T).GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo())) &&
                condition((T)value);
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
        public override string ToString() => "Any<" + ArgumentType.GetFormattedName() + ">(" + name + ")";

        #region Equality

        public bool Equals(ConditionalMatcher<T> other)
            => other != null && ReferenceEquals(condition, other.condition) && name.Equals(other.name);

        public override bool Equals(object obj) => Equals(obj as ConditionalMatcher<T>);

        public override int GetHashCode() => new HashCode().Add(condition).Add(name).ToHashCode();

        #endregion
    }
}
