using System.Collections.Generic;
using System.Threading;
using Moq.Proxy;

namespace Moq.Sdk.Tests
{
    public class Mocked : IMocked, IProxy
    {
        IMock mock;
        List<IProxyBehavior> behaviors = new List<IProxyBehavior>();

        public IMock Mock => LazyInitializer.EnsureInitialized(ref mock, () => new MockInfo(behaviors));

        public IList<IProxyBehavior> Behaviors => behaviors;
    }
}
