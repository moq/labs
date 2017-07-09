using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Moq.Proxy
{
    /// <summary>
    /// Interface implemented by all proxies.
    /// </summary>
	public interface IProxy
	{
        /// <summary>
        /// Behaviors configured for the proxy.
        /// </summary>
		ObservableCollection<IProxyBehavior> Behaviors { get; }
	}
}