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
        /// <inheritdoc />
        public IList<IMockBehavior> Behaviors { get; } = new List<IMockBehavior>();

        /// <inheritdoc />
        public IList<IMethodInvocation> Invocations { get; } = new List<IMethodInvocation>();

        /// <inheritdoc />
        public MockState State { get; } = new MockState();
    }
}