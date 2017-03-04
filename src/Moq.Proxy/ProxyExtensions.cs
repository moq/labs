namespace Moq.Proxy
{
    /// <summary>
    /// Usability functions for <see cref="IProxy"/>.
    /// </summary>
	public static class ProxyExtensions
	{
        /// <summary>
        /// Adds a behavior to a proxy.
        /// </summary>
		public static void AddBehavior(this IProxy proxy, InvokeBehavior behavior)
		{
			proxy.Behaviors.Add(ProxyBehavior.Create(behavior));
		}

        /// <summary>
        /// Inserts a behavior into the proxy behavior pipeline at the specified 
        /// index.
        /// </summary>
		public static void InsertBehavior(this IProxy proxy, int index, InvokeBehavior behavior)
		{
			proxy.Behaviors.Insert(index, ProxyBehavior.Create(behavior));
		}
	}
}
