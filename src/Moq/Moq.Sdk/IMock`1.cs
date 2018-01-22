namespace Moq.Sdk
{
    /// <summary>
    /// Provides introspection information about a mock.
    /// </summary>
    public interface IMock<T> : IMock
    {
        /// <summary>
        /// The mock object this introspection data belongs to.
        /// </summary>
        new T Object { get; }
    }
}
