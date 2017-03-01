using System.Collections.Generic;

namespace Moq.Sdk
{
    /// <summary>
    /// Interface implemented by all proxies.
    /// </summary>
	public interface IProxy
	{
        /// <summary>
        /// List of behaviors configured for the proxy.
        /// </summary>
		IList<IProxyBehavior> Behaviors { get; }
	}
}