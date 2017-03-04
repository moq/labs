namespace Moq.Sdk
{
    /// <summary>
    /// A factory for <see cref="IProxyBehavior"/> from lambdas.
    /// </summary>
    public static class ProxyBehavior
    {
        /// <summary>
        /// Creates an <see cref="AnonymousProxyBehavior"/> for the given 
        /// <see cref="InvokeBehavior"/> delegate.
        /// </summary>
        public static IProxyBehavior Create(InvokeBehavior behavior) => new AnonymousProxyBehavior(behavior);

        class AnonymousProxyBehavior : IProxyBehavior
        {
            InvokeBehavior behavior;

            public AnonymousProxyBehavior(InvokeBehavior behavior) => this.behavior = behavior;

            public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) =>
                behavior(invocation, getNext);
        }
    }
}
