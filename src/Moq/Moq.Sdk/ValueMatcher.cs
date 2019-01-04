using System;
using System.Collections;
using System.Diagnostics;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches arguments against a fixed constant value.
    /// </summary>
    public class ValueMatcher : IArgumentMatcher, IEquatable<ValueMatcher>
    {
        Tuple<Type, object> value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueMatcher"/> class.
        /// </summary>
        /// <param name="argumentType">Type of the argument to match.</param>
        /// <param name="matchValue">The value to match against.</param>
        public ValueMatcher(Type argumentType, object matchValue) => value = Tuple.Create(argumentType, matchValue);

        /// <summary>
        /// Gets the type of the argument this matcher supports.
        /// </summary>
        public Type ArgumentType => value.Item1;

        /// <summary>
        /// The value to match against invocation arguments.
        /// </summary>
        public object MatchValue => value.Item2;

        /// <summary>
        /// Evaluates whether the given value equals the <see cref="MatchValue"/> 
        /// received in the constructor, using default object equality behavior.
        /// </summary>
        public bool Matches(object value) => object.Equals(value, MatchValue);

        /// <summary>
        /// Gets a friendly representation of the object.
        /// </summary>
        /// <devdoc>
        /// We don't want to optimize code coverage for this since it's a debugger aid only. 
        /// Annotating this method with DebuggerNonUserCode achieves that.
        /// No actual behavior depends on these strings.
        /// </devdoc>
        [DebuggerNonUserCode]
        public override string ToString()
            => (IsString(ArgumentType) && MatchValue != null)
                ? "\"" + MatchValue + "\""
                : (MatchValue?.ToString() ?? "null");

        static bool IsString(Type type) => type == typeof(string) ||
            (type.IsByRef && type.HasElementType && type.GetElementType() == typeof(string));

        #region Equality

        public bool Equals(ValueMatcher other) => object.Equals(value, other?.value);

        public override bool Equals(object obj) => Equals(obj as ValueMatcher);

        public override int GetHashCode() => value.GetHashCode();

        #endregion
    }
}
