using System;
using Moq.Proxy;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MockProxyBehaviorTests
    {
        [Fact]
        public void RecordsInvocation()
        {
            var behavior = new MockProxyBehavior();

            behavior.Invoke(new MethodInvocation(new Mocked(), typeof(object).GetMethod(nameof(object.ToString))), 
                () => (m, n) => m.CreateValueReturn(null));

            Assert.Equal(1, behavior.Invocations.Count);
        }

        [Fact]
        public void ReturnsFromIMocked()
        {
            var behavior = new MockProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(
                new Mocked(), 
                typeof(Mocked).GetProperty(nameof(IMocked.Mock)).GetGetMethod()), 
                null);

            Assert.True(result.ReturnValue is IMocked);
            Assert.Equal(0, behavior.Invocations.Count);
        }

        [Fact(Skip = "Waiting on Roslyn generator.")]
        public void WhenAddingMockBehavior_ThenCanInterceptSelectively()
        {
            /*
            var calculator = (ICalculator)new DynamicProxyFactory().CreateProxy(typeof(ICalculator), new[] { typeof(IMocked) }, new object[0]);
            var behavior = new MockProxyBehavior();

            calculator.AddProxyBehavior(behavior);
            calculator.AddProxyBehavior(new DefaultValueProxyBehavior());
            calculator.AddMockBehavior(m => m.MethodBase.Name == "get_Mode", (m, n) => m.CreateValueReturn("Basic"));

            var mode = calculator.Mode;
            var add = calculator.Add(3, 2);

            Assert.Equal("Basic", mode);
            Assert.Equal(0, add);
            */
        }

        public class Mocked : IMocked
        {
            public IMock Mock => throw new NotImplementedException();
        }
    }
}