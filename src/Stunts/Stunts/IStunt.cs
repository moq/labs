using System.Collections.ObjectModel;

namespace Stunts
{
    /// <summary>
    /// Interface implemented by all stunts.
    /// </summary>
	public interface IStunt
	{
        /// <summary>
        /// Behaviors configured for the stunt.
        /// </summary>
		ObservableCollection<IStuntBehavior> Behaviors { get; }
	}
}