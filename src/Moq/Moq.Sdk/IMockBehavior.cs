using System.Collections.ObjectModel;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// An <see cref="IStuntBehavior"/> that applies a set of behaviors 
    /// selectively when the current invocation satisfies the 
    /// <see cref="IMockSetup.AppliesTo(IMethodInvocation)"/> method for 
    /// the current <see cref="Setup"/>.
    /// </summary>
    public interface IMockBehavior : IStuntBehavior
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
