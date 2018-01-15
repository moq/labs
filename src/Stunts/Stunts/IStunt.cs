using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Stunts
{
    /// <summary>
    /// Interface implemented by all stunts.
    /// </summary>
    [GeneratedCode("Stunts", ThisAssembly.Project.Properties.AssemblyVersion)] // This attribute prevents registering the "Implement through behavior pipeline" codefix.
    [CompilerGenerated]
    public interface IStunt
	{
        /// <summary>
        /// Behaviors configured for the stunt.
        /// </summary>
		ObservableCollection<IStuntBehavior> Behaviors { get; }
	}
}