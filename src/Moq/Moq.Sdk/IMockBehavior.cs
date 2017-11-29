using System.Collections.ObjectModel;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// An <see cref="IProxyBehavior"/> that applies a set of behaviors 
    /// selectively when the current invocation satisfies the 
    /// <see cref="IMockSetup.AppliesTo(IMethodInvocation)"/> method.
    /// </summary>
    public interface IMockBehavior : IProxyBehavior
    {
        /// <summary>
        /// List of behaviors that should be executed whenever the 
        /// current invocation matches the given setup.
        /// </summary>
        ObservableCollection<InvocationBehavior> Behaviors { get; }

        /// <summary>
        /// The setup corresponding to this behavior.
        /// </summary>
        IMockSetup Setup { get; }
    }
}
