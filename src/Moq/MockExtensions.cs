using Moq.Proxy;
using Moq.Sdk;

namespace Moq
{
    public static class MockExtensions
    {
        public static void Returns<T>(this object target, T value)
        {
            var invocation = CallContext<IMethodInvocation>.GetData(nameof(IMethodInvocation));

            var mock = (IMock)invocation.Target;

            mock.Invocations.Remove(invocation);

            mock.AddMockBehavior(mi => mi.MethodBase == invocation.MethodBase, (mi, next) => mi.CreateValueReturn(value));
        }
    }
}
