namespace Stunts
{
    /// <summary>
    /// A factory for a <see cref="IStuntBehavior"/> from a lambda.
    /// </summary>
    public static class StuntBehavior
    {
        /// <summary>
        /// Creates an <see cref="AnonymousProxyBehavior"/> for the given 
        /// <see cref="InvokeBehavior"/> delegate.
        /// </summary>
        /// <param name="behavior">The actual behavior that will be invoked as part of the stunt.</param>
        /// <param name="appliesTo">Whether the <paramref name="behavior"/> applies to a particular <see cref="IMethodInvocation"/>. Defaults to 'always applies'.</param>
        /// <param name="name">Optional friendly name for the behavior for diagnostics.</param>
        public static IStuntBehavior Create(InvokeBehavior behavior, AppliesTo appliesTo = null, string name = null)
            => new AnonymousProxyBehavior(behavior, appliesTo, name);

        class AnonymousProxyBehavior : IStuntBehavior
        {
            AppliesTo appliesTo;
            InvokeBehavior behavior;
            string name;

            public AnonymousProxyBehavior(InvokeBehavior behavior, AppliesTo appliesTo, string name)
            {
                this.behavior = behavior;
                this.appliesTo = appliesTo ?? new AppliesTo(invocation => true);
                this.name = name;
            }

            public bool AppliesTo(IMethodInvocation invocation) => appliesTo(invocation);

            public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior next) =>
                behavior(invocation, next);

            public override string ToString() => name ?? "<unnamed>";
        }
    }
}
