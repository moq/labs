namespace Moq.Proxy
{
    /// <summary>
    /// A <see cref="IProxyBehavior"/> implementation to use with 
    /// lambdas matching the <see cref="InvokeBehavior"/> delegate signature.
    /// </summary>
    public class AnonymousProxyBehavior : IProxyBehavior
    {
        InvokeBehavior behavior;

        /// <summary>
        /// Creates the behavior for the given delegate.
        /// </summary>
        /// <param name="behavior"></param>
        public AnonymousProxyBehavior(InvokeBehavior behavior) => this.behavior = behavior;

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) =>
            behavior(invocation, getNext);
    }
}
