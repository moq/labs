using System;
using System.Diagnostics;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Represents an invokable behavior in the <see cref="IMockBehavior.Behaviors"/> 
    /// that applies whenever the <see cref="IMockBehavior.Setup"/> is matched in a 
    /// mock runtime call.
    /// </summary>
    [DebuggerDisplay("{DisplayName}")]
    public class Behavior : IBehavior
    {
        Lazy<string> displayName;

        /// <summary>
        /// Creates an instance of the invokable behavior with the given 
        /// delegate and friendly display name.
        /// </summary>
        public Behavior(InvokeBehavior invoke, string displayName)
            : this(invoke, new Lazy<string>(() => displayName))
        {
        }

        /// <summary>
        /// Creates an instance of the invokable behavior with the given 
        /// delegate and friendly display name.
        /// </summary>
        /// <remarks>
        /// Use this constructor overload whenever constructing the display
        /// name is somewhat expensive.
        /// </remarks>
        public Behavior(InvokeBehavior invoke, Lazy<string> displayName)
        {
            Invoke = invoke;
            this.displayName = displayName;
        }

        /// <summary>
        /// The delegate that implements the actual behavior.
        /// </summary>
        public InvokeBehavior Invoke { get; }

        /// <summary>
        /// A friendly display name that describes what invoking the 
        /// <see cref="Invoke"/> delegate will do.
        /// </summary>
        public string DisplayName => displayName.Value;

        /// <summary>
        /// Returns the <see cref="DisplayName"/>.
        /// </summary>
        public override string ToString() => DisplayName;
    }
}