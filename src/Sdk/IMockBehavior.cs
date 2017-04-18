using Moq.Proxy;

namespace Moq.Sdk
{
    public interface IMockBehavior : IProxyBehavior
    {
        bool AppliesTo(IMethodInvocation invocation);
    }
}
