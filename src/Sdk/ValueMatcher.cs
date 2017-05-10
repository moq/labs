using System;

namespace Moq.Sdk
{
    /// <summary>
    /// Matches arguments against a fixed constant value.
    /// </summary>
    public class ValueMatcher : IArgumentMatcher
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

        public override bool Equals(object obj) => Equals(this, obj as ValueMatcher);

        public override int GetHashCode() => value.GetHashCode();

        static bool Equals(ValueMatcher obj1, ValueMatcher obj2)
        {
            if (object.Equals(null, obj1) ||
                object.Equals(null, obj2) ||
                obj1.GetType() != obj2.GetType())
                return false;

            if (object.ReferenceEquals(obj1, obj2)) return true;

            return obj1.value.Equals(obj2.value);
        }

        public override string ToString()
            => (ArgumentType == typeof(string) && MatchValue != null) 
                ? "\"" + MatchValue + "\"" 
                : (MatchValue?.ToString() ?? "null");
    }
}
