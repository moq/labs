using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Moq.Proxy;
using Moq.Sdk;

namespace Moq
{
    public static class Mock
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [ProxyGenerator]
        public static T Of<T>(Assembly proxiesAssembly, params Type[] additionalInterfaces)
        {
            var proxy = (IProxy)ProxyFactory.Default.CreateProxy(proxiesAssembly, typeof(T), additionalInterfaces.Concat(new[] { typeof(IMocked) }), new object[0]);

            proxy.Behaviors.Add(new MockProxyBehavior());
            proxy.Behaviors.Add(new DefaultValueProxyBehavior());

            return (T)proxy;
        }
    }
}
