using Moq.Proxy;
using Moq.Sdk;

namespace Moq
{
    public static class Mock
    {
        public static T Of<T>(IProxyFactory factory  = null)
        {
            if (factory == null)
                factory = ProxyFactory.Default;

            var proxy = (IProxy)factory.CreateProxy(typeof(T), new[] { typeof(IMock), typeof(IMocked) }, new object[0]);

            proxy.Behaviors.Add(new MockProxyBehavior());
            proxy.Behaviors.Add(new DefaultValueProxyBehavior());

            return (T)proxy;
        }
    }
}
