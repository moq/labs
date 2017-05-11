using System;
using Moq.Proxy;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MockBehaviorTests
    {
        [Fact]
        public void RecordsInvocation()
        {
            var behavior = new MockBehavior();
            var mock = new Mocked();

            behavior.Invoke(new MethodInvocation(mock, typeof(object).GetMethod(nameof(object.ToString))), 
                () => (m, n) => m.CreateValueReturn(null));

            Assert.Equal(1, mock.Mock.Invocations.Count);
        }

        [Fact]
        public void ThrowsForNonIMocked()
        {
            var behavior = new MockBehavior();

            Assert.Throws<ArgumentException>(() => behavior.Invoke(new MethodInvocation(
                new object(),
                typeof(Mocked).GetProperty(nameof(IMocked.Mock)).GetGetMethod()),
                null));
        }

        [Fact]
        public void WhenAddingMockBehavior_ThenCanInterceptSelectively()
        {
            var calculator = new ICalculatorProxy();
            var behavior = new MockBehavior();

            calculator.AddBehavior(behavior);
            calculator.AddBehavior(new DefaultValueBehavior());
            calculator.AddBehavior(m => m.MethodBase.Name == "get_Mode", (m, n) => m.CreateValueReturn("Basic"));

            var mode = calculator.Mode;
            var add = calculator.Add(3, 2);

            Assert.Equal("Basic", mode);
            Assert.Equal(0, add);
        }

        public class Mocked : IMocked
        {
            public IMock Mock { get; } = new MockInfo();
        }
    }
}