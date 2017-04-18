using System;
using System.Collections.Generic;
using Xunit;

namespace Moq.Proxy.Tests
{
    public class ProxyExtensionsTests
    {
        [Fact]
        public void WhenAddingProxyBehavior_ThenCanAddLambda()
        {
            var proxy = new TestProxy();

            proxy.AddProxyBehavior((m, n) => null);

            Assert.Equal(1, proxy.Behaviors.Count);
        }

        [Fact]
        public void WhenAddingProxyBehavior_ThenCanAddInterface()
        {
            var proxy = new TestProxy();

            proxy.AddProxyBehavior(new TestProxyBehavior());

            Assert.Equal(1, proxy.Behaviors.Count);
        }

        [Fact]
        public void WhenAddingProxyBehaviorToObject_ThenCanAddLambda()
        {
            object proxy = new TestProxy();

            proxy.AddProxyBehavior((m, n) => null);

            Assert.Equal(1, ((IProxy)proxy).Behaviors.Count);
        }

        [Fact]
        public void WhenAddingProxyBehaviorToObject_ThenCanAddInterface()
        {
            object proxy = new TestProxy();

            proxy.AddProxyBehavior(new TestProxyBehavior());

            Assert.Equal(1, ((IProxy)proxy).Behaviors.Count);
        }

        [Fact]
        public void WhenInsertingProxyBehavior_ThenCanAddLambda()
        {
            var proxy = new TestProxy();

            proxy.AddProxyBehavior((m, n) => null);
            proxy.InsertProxyBehavior(0, (m, n) => throw new NotImplementedException());

            Assert.Equal(2, proxy.Behaviors.Count);
            Assert.Throws<NotImplementedException>(() => proxy.Behaviors[0].Invoke(null, null));
        }

        [Fact]
        public void WhenInsertingProxyBehavior_ThenCanAddInterface()
        {
            var proxy = new TestProxy();
            var behavior = new TestProxyBehavior();

            proxy.AddProxyBehavior((m, n) => null);
            proxy.InsertProxyBehavior(0, behavior);

            Assert.Equal(2, proxy.Behaviors.Count);
            Assert.Same(behavior, proxy.Behaviors[0]);
        }

        [Fact]
        public void WhenInsertingProxyBehaviorToObject_ThenCanAddLambda()
        {
            object proxy = new TestProxy();

            proxy.AddProxyBehavior((m, n) => null);
            proxy.InsertProxyBehavior(0, (m, n) => throw new NotImplementedException());

            Assert.Equal(2, ((IProxy)proxy).Behaviors.Count);
            Assert.Throws<NotImplementedException>(() => ((IProxy)proxy).Behaviors[0].Invoke(null, null));
        }

        [Fact]
        public void WhenInsertingProxyBehaviorToObject_ThenCanAddInterface()
        {
            object proxy = new TestProxy();
            var behavior = new TestProxyBehavior();

            proxy.AddProxyBehavior((m, n) => null);
            proxy.InsertProxyBehavior(0, behavior);

            Assert.Equal(2, ((IProxy)proxy).Behaviors.Count);
            Assert.Same(behavior, ((IProxy)proxy).Behaviors[0]);
        }

        [Fact]
        public void WhenAddingProxyBehaviorToObjectWithLambda_ThenThrowsIfNotProxy() =>
            Assert.Throws<ArgumentException>(() => new object().AddProxyBehavior((m, n) => null));

        [Fact]
        public void WhenAddingProxyBehaviorToObjectWithInterface_ThenThrowsIfNotProxy() =>
            Assert.Throws<ArgumentException>(() => new object().AddProxyBehavior(new TestProxyBehavior()));

        [Fact]
        public void WhenInsertingProxyBehaviorToObjectWithLambda_ThenThrowsIfNotProxy() =>
            Assert.Throws<ArgumentException>(() => new object().InsertProxyBehavior(0, (m, n) => null));

        [Fact]
        public void WhenInsertingProxyBehaviorToObjectWithInterface_ThenThrowsIfNotProxy() =>
            Assert.Throws<ArgumentException>(() => new object().InsertProxyBehavior(0, new TestProxyBehavior()));

        class TestProxyBehavior : IProxyBehavior
        {
            public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) => null;
        }

        class TestProxy : IProxy
        {
            public IList<IProxyBehavior> Behaviors { get; } = new List<IProxyBehavior>();
        }
    }
}
