using System;
using System.Diagnostics;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// A factory for an <see cref="IMockBehavior"/> from a delegate (a.k.a. anonymous behavior).
    /// </summary>
    public static class MockBehavior
    {
        /// <summary>
        /// Creates an <see cref="IMockBehavior"/> for the given delegate behavior.
        /// </summary>
        /// <param name="behavior">The behavior to execute.</param>
        /// <param name="displayName">A friendly display name for the behavior.</param>
        public static IMockBehavior Create(InvokeBehavior behavior, string displayName)
            => new AnonymousMockBehavior(behavior, displayName);

        /// <summary>
        /// Creates an <see cref="IMockBehavior"/> for the given delegate behavior.
        /// </summary>
        /// <param name="behavior">The behavior to execute.</param>
        /// <param name="displayName">A friendly display name for the behavior.</param>
        public static IMockBehavior Create(InvokeBehavior behavior, Lazy<string> displayName)
            => new AnonymousMockBehavior(behavior, displayName);

        /// <summary>
        /// Represents an executable behavior in the <see cref="IMockBehaviorPipeline.Behaviors"/>.
        /// </summary>
        [DebuggerDisplay("{DisplayName}")]
        class AnonymousMockBehavior : IMockBehavior
        {
            readonly Lazy<string> displayName;
            readonly InvokeBehavior behavior;

            /// <summary>
            /// Creates an instance of the invokable behavior with the given 
            /// delegate and friendly display name.
            /// </summary>
            public AnonymousMockBehavior(InvokeBehavior invoke, string displayName)
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
            public AnonymousMockBehavior(InvokeBehavior invoke, Lazy<string> displayName)
            {
                behavior = invoke;
                this.displayName = displayName;
            }

            /// <summary>
            /// Executes the delegate received in the constructor.
            /// </summary>
            public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next) => behavior(invocation, next);

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
}