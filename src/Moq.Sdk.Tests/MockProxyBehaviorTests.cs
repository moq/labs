using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq.Proxy;
using Moq.Proxy.Dynamic;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MockProxyBehaviorTests
    {
        [Fact]
        public void RecordsInvocation()
        {
            var behavior = new MockProxyBehavior();

            behavior.Invoke(new MethodInvocation(new object(), typeof(object).GetMethod(nameof(object.ToString))), 
                () => (m, n) => m.CreateValueReturn(null));

            var ms = typeof(IMock).GetMethods();
            Assert.Equal(1, behavior.Invocations.Count);
        }

        [Fact]
        public void ReturnsFromIMocked()
        {
            var behavior = new MockProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(
                new object(), 
                typeof(IMocked).GetProperty(nameof(IMocked.Mock)).GetGetMethod()), 
                null);

            Assert.True(result.ReturnValue is IMock);
            Assert.Equal(0, behavior.Invocations.Count);
        }

        [Theory]
        [MemberData("GetIMockMethods")]
        public void ForwardsIMockInvocations(MethodInfo method)
        {
            var behavior = new MockProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method), null);

            if (method.ReturnType != typeof(void))
                Assert.Same(method.Invoke(behavior, new object[0]), result.ReturnValue);

            Assert.Equal(0, behavior.Invocations.Count);
        }

        public static IEnumerable<object[]> GetIMockMethods => typeof(IMock)
            .GetMethods()
            .Select(method => new object[] { method })
            .ToArray();
    }

    //[Fact]
    //public void WhenGettingProperty_ThenCanReturnFixedValue()
    //{
    //    var behavior = new MockBehavior();
    //}
}