using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Throws for all invocations performed, since it means a 
    /// mock behavior could not be applied before reaching this 
    /// fallback behavior.
    /// </summary>
    public class StrictMockBehavior : IProxyBehavior
    {
        /// <summary>
        /// Always returns <see langword="true" />
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => true;

        /// <summary>
        /// Throws <see cref="StrictMockException"/>.
        /// </summary>
        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) => throw new StrictMockException();
    }
}
