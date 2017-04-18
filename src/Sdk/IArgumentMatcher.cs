using System;

namespace Moq.Sdk
{
    /// <summary>
    /// Interface implemented by argument matching strategies.
    /// </summary>
    public interface IArgumentMatcher
    {
        /// <summary>
        /// Gets the type of the argument this matcher supports.
        /// </summary>
        Type ArgumentType { get; }

        /// <summary>
        /// Evaluates whether the given value matches this instance.
        /// </summary>
        bool Matches(object value);
    }
}
