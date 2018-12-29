using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Provides configuration information for a mock.
    /// </summary>
    public interface IMoq<T> : IMoq
    {
        /// <summary>
        /// Access the underlying introspection data for the mock.
        /// </summary>
        new IMock<T> Sdk { get; }
    }
}
