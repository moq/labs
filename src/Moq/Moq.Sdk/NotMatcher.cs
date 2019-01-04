using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches an argument with the given type <typeparamref name="T"/>, 
    /// as long as it satisfies a given condition.
    /// </summary>
    /// <typeparam name="T">Type of argument being conditioned.</typeparam>
    public class NotMatcher<T> : IArgumentMatcher, IEquatable<NotMatcher<T>>
    {
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

        /// <summary>
        /// Gets a friendly representation of the object.
        /// </summary>
        /// <devdoc>
        /// We don't want to optimize code coverage for this since it's a debugger aid only. 
        /// Annotating this method with DebuggerNonUserCode achieves that.
        /// No actual behavior depends on these strings.
        /// </devdoc>
        [DebuggerNonUserCode]
        public override string ToString() => "!" + (value is string ? "\"" + value + "\"" : value.ToString());

        #region Equality

        public bool Equals(NotMatcher<T> other) => other == null ? false : 
            ArgumentType == other.ArgumentType && object.Equals(value, other.value);

        public override bool Equals(object obj) => Equals(obj as NotMatcher<T>);

        public override int GetHashCode() 
            => new HashCode().Add(ArgumentType).Add(value == null ? 0 : value.GetHashCode()).ToHashCode();

        #endregion
    }
}
