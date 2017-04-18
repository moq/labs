using System;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MoqTests
    {
        [Fact(Skip = "Waiting on Roslyn generator.")]
        public void RecordsInvocation()
        {
            var calculator = Moq.Of<ICalculator>();

            calculator.Mode = "Basic";

            var mock = ((IMocked)calculator).Mock;

            var invocations = mock.Invocations;

            Assert.Equal(1, invocations.Count);
        }

        [Fact(Skip = "Waiting on Roslyn generator.")]
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
            throw new NotImplementedException();
            //var proxy = (IProxy)new DynamicProxyFactory().CreateProxy(typeof(T), new[] { typeof(IMocked), typeof(IMocked) }, new object[0]);

            //proxy.Behaviors.Add(new MockProxyBehavior());
            //proxy.Behaviors.Add(new DefaultValueProxyBehavior());

            //return (T)proxy;
        }
    }

}