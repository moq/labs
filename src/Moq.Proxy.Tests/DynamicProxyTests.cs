using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Moq.Proxy.Dynamic;
using Moq.Proxy;
using Xunit;

namespace Moq.Proxy.Tests
{
    public class DynamicProxyTests
    {
        [Fact]
        public void WhenCreatingProxy_ThenImplementsIProxy()
        {
            var proxy = new DynamicProxyFactory().CreateProxy(typeof(IFoo), new Type[0], new object[0]);

            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<IProxy>(proxy);
        }

        [Fact]
        public void WhenCreatingProxy_ThenCanAddBehavior()
        {
            var proxy = (IProxy)new DynamicProxyFactory().CreateProxy(typeof(IFoo), new Type[0], new object[0]);

            var expected = 5;

            proxy.AddBehavior((m, n) => m.CreateValueReturn(expected, 2, 3));

            var foo = (IFoo)proxy;

            var actual = foo.Add(2, 3);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WhenCreatingProxy_ThenCanAddBehaviorThatThrows()
        {
            var proxy = (IProxy)new DynamicProxyFactory().CreateProxy(typeof(IFoo), new Type[0], new object[0]);

            proxy.AddBehavior((m, n) => m.CreateExceptionReturn(new ArgumentException()));

            var foo = (IFoo)proxy;

            Assert.Throws<ArgumentException>(() => foo.Add(2, 3));
        }

        public interface IFoo
        {
            int Add(int x, int y);
        }
    }
}
