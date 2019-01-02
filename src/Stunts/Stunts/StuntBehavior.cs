using System.Diagnostics;

namespace Stunts
{
    /// <summary>
    /// A factory for a <see cref="IStuntBehavior"/> from a delegate (a.k.a. anonymous behavior).
    /// </summary>
    public static class StuntBehavior
    {
        /// <summary>
        /// Creates an <see cref="IStuntBehavior"/> for the given 
        /// <see cref="ExecuteDelegate"/> delegate.
        /// </summary>
        /// <param name="behavior">The actual behavior that will be invoked as part of the stunt.</param>
        /// <param name="appliesTo">Whether the <paramref name="behavior"/> applies to a particular <see cref="IMethodInvocation"/>. Defaults to 'always applies'.</param>
        /// <param name="name">Optional friendly name for the behavior for diagnostics.</param>
        public static IStuntBehavior Create(ExecuteDelegate behavior, AppliesTo appliesTo = null, string name = null)
            => new AnonymousStuntBehavior(behavior, appliesTo, name);

        class AnonymousStuntBehavior : IStuntBehavior
        {
            AppliesTo appliesTo;
            ExecuteDelegate behavior;
            string name;

            public AnonymousStuntBehavior(ExecuteDelegate behavior, AppliesTo appliesTo, string name)
            {
                this.behavior = behavior;
                this.appliesTo = appliesTo ?? new AppliesTo(invocation => true);
                this.name = name;
            }

            public bool AppliesTo(IMethodInvocation invocation) => appliesTo(invocation);

            public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next) =>
                behavior(invocation, next);

            /// <summary>
            /// Gets a friendly representation of the object.
            /// </summary>
            /// <devdoc>
            /// We don't want to optimize code coverage for this since it's a debugger aid only. 
            /// Annotating this method with DebuggerNonUserCode achieves that.
            /// No actual behavior depends on these strings.
            /// </devdoc>
            [DebuggerNonUserCode]
            public override string ToString() => name ?? "<unnamed>";
        }
    }
}
