using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq.Proxy;
using Moq.Proxy.Dynamic;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MockBehaviorTests
    {
        [Fact]
        public void RecordsInvocation()
        {
            var behavior = new MockBehavior();

            behavior.Invoke(new MethodInvocation(new object(), typeof(object).GetMethod(nameof(object.ToString))), 
                () => (m, n) => m.CreateValueReturn(null));

            var ms = typeof(IMock).GetMethods();
            Assert.Equal(1, behavior.Invocations.Count);
        }

        [Theory]
        [MemberData("GetIMockMethods")]
        public void ForwardsIMockInvocations(MethodInfo method)
        {
            var behavior = new MockBehavior();

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