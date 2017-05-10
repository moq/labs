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

            behavior.Invoke(new MethodInvocation(new Mocked(), typeof(object).GetMethod(nameof(object.ToString))), 
                () => (m, n) => m.CreateValueReturn(null));

            Assert.Equal(1, behavior.Invocations.Count);
        }

        [Fact]
        public void ReturnsFromIMocked()
        {
            var behavior = new MockBehavior();

            var result = behavior.Invoke(new MethodInvocation(
                new Mocked(), 
                typeof(Mocked).GetProperty(nameof(IMocked.Mock)).GetGetMethod()), 
                null);

            Assert.True(result.ReturnValue is IMock);
            Assert.Equal(0, behavior.Invocations.Count);
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
            public IMock Mock => throw new NotImplementedException();
        }
    }
}