using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Provides configuration and introspection information for a mock.
    /// </summary>
    public interface IMoq<T> : IMoq, IMock<T>, IFluentInterface
    {
    }
}
