using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// An <see cref="IProxyBehavior"/> that applies selectively to the 
    /// current <see cref="IMethodInvocation"/> depending on the result 
    /// of evaluating the <see cref="AppliesTo(IMethodInvocation)"/> method 
    /// by the <see cref="MockBehavior"/>.
    /// </summary>
    public interface IMockBehavior : IProxyBehavior
    {
        /// <summary>
        /// Determines whether the behavior applies to the given 
        /// <see cref="IMethodInvocation"/>.
        /// </summary>
        /// <param name="invocation">The invocation to evaluate the 
        /// behavior against.</param>
        bool AppliesTo(IMethodInvocation invocation);
    }
}
