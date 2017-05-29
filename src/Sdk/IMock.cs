using System.Collections.Generic;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Provides introspection information about a mock.
    /// </summary>
    public interface IMock : IProxy
    {
        /// <summary>
        /// Invocations performed on the mock.
        /// </summary>
        IList<IMethodInvocation> Invocations { get; }

        /// <summary>
        /// Arbitrary state associated with a mock instance.
        /// </summary>
        MockState State { get; }
    }
}
