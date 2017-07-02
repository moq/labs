using System;
using System.Collections.Generic;
using System.Threading;
using Moq.Proxy;
using Xunit;
using Proxies;

namespace Moq.Sdk.Tests
{
    public class MockBehaviorTests
    {
        [Fact]
        public void RecordsInvocation()
        {
            var behavior = new MockTrackingBehavior();
            var mock = new Mocked();

            behavior.Invoke(new MethodInvocation(mock, typeof(object).GetMethod(nameof(object.ToString))),
                () => (m, n) => m.CreateValueReturn(null));

            Assert.Equal(1, mock.Mock.Invocations.Count);
        }

        [Fact]
        public void ThrowsForNonIMocked()
        {
            var behavior = new MockTrackingBehavior();

            Assert.Throws<ArgumentException>(() => behavior.Invoke(new MethodInvocation(
                new object(),
                typeof(Mocked).GetProperty(nameof(IMocked.Mock)).GetGetMethod()),
                null));
        }

        [Fact]
        public void WhenAddingMockBehavior_ThenCanInterceptSelectively()
        {
            var calculator = new ICalculatorProxy();
            var behavior = new MockTrackingBehavior();

            calculator.AddBehavior(behavior);
            calculator.AddBehavior((m, n) => m.CreateValueReturn("Basic"), m => m.MethodBase.Name == "get_Mode");
            calculator.AddBehavior(new DefaultValueBehavior());

            var mode = calculator.Mode;
            var add = calculator.Add(3, 2);

            Assert.Equal("Basic", mode);
            Assert.Equal(0, add);
        }
    }
}