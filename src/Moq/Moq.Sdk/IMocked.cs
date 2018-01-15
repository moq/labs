using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace Moq.Sdk
{
    /// <summary>
    /// Interface implemented by mocks that allows accessing 
    /// the <see cref="IMock"/> interface for introspecting 
    /// a mock instance.
    /// </summary>
    [GeneratedCode("Moq", ThisAssembly.Project.Properties.AssemblyVersion)] // This attribute prevents registering the "Implement through behavior pipeline" codefix.
    [CompilerGenerated]
    public interface IMocked
    {
        /// <summary>
        /// The introspection information for the current mock.
        /// </summary>
        IMock Mock { get; }
    }
}