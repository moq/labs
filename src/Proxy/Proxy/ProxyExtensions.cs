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
		public static IProxy AddBehavior(this IProxy proxy, InvokeBehavior behavior, string name = null)
        {
            proxy.Behaviors.Add(ProxyBehavior.Create(behavior, name));
            return proxy;
        }

        /// <summary>
        /// Adds a behavior to a proxy.
        /// </summary>
		public static IProxy AddBehavior(this IProxy proxy, IProxyBehavior behavior)
        {
            proxy.Behaviors.Add(behavior);
            return proxy;
        }

        /// <summary>
        /// Adds a behavior to a proxy.
        /// </summary>
		public static TProxy AddBehavior<TProxy>(this TProxy proxy, InvokeBehavior behavior, string name = null)
        {
            // We can't just add a constraint to the method signature, because 
            // proxies are typically geneated and don't expose the IProxy interface directly.
            if (proxy is IProxy target)
                target.Behaviors.Add(ProxyBehavior.Create(behavior, name));
            else
                throw new ArgumentException(nameof(proxy));

            return proxy;
        }

        /// <summary>
        /// Adds a behavior to a proxy.
        /// </summary>
		public static TProxy AddBehavior<TProxy>(this TProxy proxy, IProxyBehavior behavior)
        {
            if (proxy is IProxy target)
                target.Behaviors.Add(behavior);
            else
                throw new ArgumentException(nameof(proxy));

            return proxy;
        }

        /// <summary>
        /// Inserts a behavior into the proxy behavior pipeline at the specified 
        /// index.
        /// </summary>
		public static IProxy InsertBehavior(this IProxy proxy, int index, InvokeBehavior behavior, string name = null)
        {
            proxy.Behaviors.Insert(index, ProxyBehavior.Create(behavior, name));
            return proxy;
        }

        /// <summary>
        /// Inserts a behavior into the proxy behavior pipeline at the specified 
        /// index.
        /// </summary>
        public static IProxy InsertBehavior(this IProxy proxy, int index, IProxyBehavior behavior)
        {
            proxy.Behaviors.Insert(index, behavior);
            return proxy;
        }

        /// <summary>
        /// Inserts a behavior into the proxy behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static TProxy InsertBehavior<TProxy>(this TProxy proxy, int index, InvokeBehavior behavior, string name = null)
        {
            if (proxy is IProxy target)
                target.Behaviors.Insert(index, ProxyBehavior.Create(behavior, name));
            else
                throw new ArgumentException(nameof(proxy));

            return proxy;
        }

        /// <summary>
        /// Inserts a behavior into the proxy behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static TProxy InsertBehavior<TProxy>(this TProxy proxy, int index, IProxyBehavior behavior)
        {
            if (proxy is IProxy target)
                target.Behaviors.Insert(index, behavior);
            else
                throw new ArgumentException(nameof(proxy));

            return proxy;
        }
    }
}
