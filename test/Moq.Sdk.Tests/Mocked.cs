using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Moq.Proxy;

namespace Moq.Sdk.Tests
{
    public class Mocked : IMocked, IProxy
    {
        IMock mock;
        ObservableCollection<IProxyBehavior> behaviors = new ObservableCollection<IProxyBehavior>();

        public IMock Mock => LazyInitializer.EnsureInitialized(ref mock, () => new MockInfo(this));

        public ObservableCollection<IProxyBehavior> Behaviors => behaviors;
    }
}
