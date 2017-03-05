using System;

namespace Moq.Proxy
{
    /// <summary>
    /// Usability functions for working with proxies.
    /// </summary>
	public static class ProxyExtensions
    {
        /// <summary>
        /// Adds a behavior to a proxy.
        /// </summary>
		public static void AddProxyBehavior(this IProxy proxy, InvokeBehavior behavior)
        {
            proxy.Behaviors.Add(ProxyBehavior.Create(behavior));
        }

        /// <summary>
        /// Adds a behavior to a proxy.
        /// </summary>
		public static void AddProxyBehavior(this IProxy proxy, IProxyBehavior behavior)
        {
            proxy.Behaviors.Add(behavior);
        }

        /// <summary>
        /// Adds a behavior to a proxy.
        /// </summary>
		public static void AddProxyBehavior(this object proxy, InvokeBehavior behavior)
        {
            if (proxy is IProxy target)
                target.Behaviors.Add(ProxyBehavior.Create(behavior));
            else
                throw new ArgumentException(nameof(proxy));
        }

        /// <summary>
        /// Adds a behavior to a proxy.
        /// </summary>
		public static void AddProxyBehavior(this object proxy, IProxyBehavior behavior)
        {
            if (proxy is IProxy target)
                target.Behaviors.Add(behavior);
            else
                throw new ArgumentException(nameof(proxy));
        }

        /// <summary>
        /// Inserts a behavior into the proxy behavior pipeline at the specified 
        /// index.
        /// </summary>
		public static void InsertProxyBehavior(this IProxy proxy, int index, InvokeBehavior behavior)
        {
            proxy.Behaviors.Insert(index, ProxyBehavior.Create(behavior));
        }

        /// <summary>
        /// Inserts a behavior into the proxy behavior pipeline at the specified 
        /// index.
        /// </summary>
        public static void InsertProxyBehavior(this IProxy proxy, int index, IProxyBehavior behavior)
        {
            proxy.Behaviors.Insert(index, behavior);
        }

        /// <summary>
        /// Inserts a behavior into the proxy behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static void InsertProxyBehavior(this object proxy, int index, InvokeBehavior behavior)
        {
            if (proxy is IProxy target)
                target.Behaviors.Insert(index, ProxyBehavior.Create(behavior));
            else
                throw new ArgumentException(nameof(proxy));
        }

        /// <summary>
        /// Inserts a behavior into the proxy behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static void InsertProxyBehavior(this object proxy, int index, IProxyBehavior behavior)
        {
            if (proxy is IProxy target)
                target.Behaviors.Insert(index, behavior);
            else
                throw new ArgumentException(nameof(proxy));
        }
    }
}
