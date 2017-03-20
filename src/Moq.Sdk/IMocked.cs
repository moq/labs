namespace Moq.Sdk
{
    /// <summary>
    /// Interface implemented by mocks that allows accessing 
    /// the <see cref="IMocked"/> interface for introspecting 
    /// a mock instance.
    /// </summary>
    public interface IMocked
    {
        /// <summary>
        /// The introspection information for the current mock.
        /// </summary>
        IMock Mock { get; }
    }
}
