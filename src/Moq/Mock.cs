using Moq.Proxy;
using Moq.Sdk;

namespace Moq
{
    public static class Mock
    {
        [ProxyGenerator]
        public static T Of<T>(IProxyFactory factory  = null)
        {
            if (factory == null)
                factory = ProxyFactory.Default;

            var proxy = (IProxy)factory.CreateProxy(typeof(T), new[] { typeof(IMocked) }, new object[0]);

            proxy.Behaviors.Add(new MockProxyBehavior());
            proxy.Behaviors.Add(new DefaultValueProxyBehavior());

            return (T)proxy;
        }
    }
}
