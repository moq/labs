using System.Linq;
using System.Collections.Generic;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// An <see cref="IProxyBehavior"/> that performs argument matching and 
    /// invocation interception based on behavior configured for a mock.
    /// </summary>
    public class MockProxyBehavior : IProxyBehavior, IMock
    {
        public IList<IMethodInvocation> Invocations { get; } = new List<IMethodInvocation>();

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            // TODO: this can be optimized with compiled delegates.
            if (invocation.MethodBase.DeclaringType == typeof(IMock))
                return invocation.CreateValueReturn(invocation.MethodBase.Invoke(this, invocation.Arguments.ToArray()));

            if (invocation.MethodBase.DeclaringType == typeof(IMocked))
                return invocation.CreateValueReturn(this);

            Invocations.Add(invocation);

            return getNext().Invoke(invocation, getNext);
        }
    }
}
