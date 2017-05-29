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
        /// The setup corresponding to this behavior.
        /// </summary>
        IMockSetup Setup { get; }
    }
}
