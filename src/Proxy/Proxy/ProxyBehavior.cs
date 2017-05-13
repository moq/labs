namespace Moq.Proxy
{
    /// <summary>
    /// A factory for a <see cref="IProxyBehavior"/> from a lambda.
    /// </summary>
    public static class ProxyBehavior
    {
        /// <summary>
        /// Creates an <see cref="AnonymousProxyBehavior"/> for the given 
        /// <see cref="InvokeBehavior"/> delegate.
        /// </summary>
        public static IProxyBehavior Create(InvokeBehavior behavior, string name = null) => new AnonymousProxyBehavior(behavior, name);

        class AnonymousProxyBehavior : IProxyBehavior
        {
            InvokeBehavior behavior;
            string name;

            public AnonymousProxyBehavior(InvokeBehavior behavior, string name)
            {
                this.behavior = behavior;
                this.name = name;
            }

            public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) =>
                behavior(invocation, getNext);

            public override string ToString() => name ?? "custom";
        }
    }
}
