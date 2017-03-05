using Moq.Proxy;
using Moq.Proxy.Dynamic;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MoqTests
    {
        [Fact]
        public void RecordsInvocation()
        {
            var calculator = Moq.Of<ICalculator>();

            calculator.Mode = "Basic";

            var mock = (IMock)calculator;

            var invocations = mock.Invocations;

            Assert.Equal(1, invocations.Count);
        }

        [Fact]
        public void AbstractMethodContinues()
        {
            var target = Moq.Of<Foo>();

            target.Do();
        }
    }

    public abstract class Foo
    {
        public abstract void Do();
    }

    public static class Moq
    {
        public static T Of<T>()
        {
            var proxy = (IProxy)new DynamicProxyFactory().CreateProxy(typeof(T), new[] { typeof(IMock) }, new object[0]);

            proxy.Behaviors.Add(new MockProxyBehavior());
            proxy.Behaviors.Add(new DefaultValueProxyBehavior());

            return (T)proxy;
        }
    }

}