using System;
using System.ComponentModel;
using System.Reflection;
using Moq.Proxy;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Main factory for mocks.
    /// </summary>
    public static class Mock
    {
        /// <summary>
        /// Entry point method that causes proxies to be generated for 
        /// instances of <typeparamref name="T"/> at compile time, and 
        /// instantiated and properly configured at run time. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proxiesAssembly">The assembly containing the proxy 
        /// types generated during compile time.</param>
        /// <param name="additionalInterfaces">Additional interfaces the 
        /// proxy implements.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [ProxyGenerator]
        public static T Of<T>(Assembly proxiesAssembly, params Type[] additionalInterfaces)
        {
            var proxy = (IProxy)ProxyFactory.Default.CreateProxy(proxiesAssembly, typeof(T), additionalInterfaces, new object[0]);

            proxy.Behaviors.Add(new MockTrackingBehavior());
            // TODO: depending on Strict/Loose mock behavior, we should either 
            // add DefaultValueBehavior or StrictMockBehavior
            proxy.Behaviors.Add(new DefaultValueBehavior());

            return (T)proxy;
        }
    }
}
