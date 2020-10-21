using System;
using System.Diagnostics;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches an argument with the given type <typeparamref name="T"/>, 
    /// as long as it is not equal to the initial value.
    /// </summary>
    /// <typeparam name="T">Type of argument being conditioned.</typeparam>
    public class NotMatcher<T> : IArgumentMatcher, IEquatable<NotMatcher<T>>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly T value;

        /// <summary>
        /// Initalizes the matcher with the value to check for.
        /// </summary>
        public NotMatcher(T value) => this.value = value;

        /// <summary>
        /// Gets the type of the argument this matcher supports.
        /// </summary>
        public Type ArgumentType => typeof(T);

        /// <summary>
        /// Evaluates whether the given value matches this instance, which is always 
        /// the case unless the initial value equals the <paramref name="value"/>.
        /// </summary>
        public bool Matches(object value) => !(Object.Equals(this.value, value));

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

        /// <inheritdoc />
        public bool Equals(NotMatcher<T> other) => other == null ? false : 
            ArgumentType == other.ArgumentType && object.Equals(value, other.value);

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as NotMatcher<T>);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(ArgumentType, value == null ? 0 : value.GetHashCode());

        #endregion
    }
}
