using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Moq.Sdk
{
    /// <summary>
    /// Interface implemented by mocks that allows accessing 
    /// the <see cref="IMock"/> interface for introspecting 
    /// a mock instance.
    /// </summary>
    [CompilerGenerated]
    public interface IMocked
    {
        /// <summary>
        /// The introspection information for the current mock.
        /// </summary>
        [DebuggerDisplay("Invocations = {Mock.Invocations.Count}", Name = nameof(IMocked) + "." + nameof(IMocked.Mock))]
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        IMock Mock { get; }
    }
}