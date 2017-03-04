using System.Threading;

namespace Moq.Proxy
{
    /// <summary>
    /// Allows accessing the default <see cref="IProxyFactory"/> to use to 
    /// create proxies.
    /// </summary>
    public static class ProxyFactory
    {
        static IProxyFactory factory;

        /// <summary>
        /// Gets or sets the default <see cref="IProxyFactory"/> to use 
        /// to create proxies.
        /// </summary>
        public static IProxyFactory Default
        {
            get => factory;
            set => factory = value;
        }
    }
}
