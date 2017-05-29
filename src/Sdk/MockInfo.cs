using System.Collections.Generic;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IMock"/> for inspecting 
    /// a mock.
    /// </summary>
    public class MockInfo : IMock
    {
        public MockInfo(IProxy proxy) => Behaviors = proxy.Behaviors;

        public MockInfo(IList<IProxyBehavior> behaviors) => Behaviors = behaviors;

        /// <inheritdoc />
        public IList<IProxyBehavior> Behaviors { get; }

        /// <inheritdoc />
        public IList<IMethodInvocation> Invocations { get; } = new List<IMethodInvocation>();

        /// <inheritdoc />
        public MockState State { get; } = new MockState();
    }
}