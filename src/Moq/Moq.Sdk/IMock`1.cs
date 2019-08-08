namespace Moq.Sdk
{
    /// <summary>
    /// Provides introspection information about a mock.
    /// </summary>
    public interface IMock<out T> : IMock, IFluentInterface where T : class
    {
        /// <summary>
        /// The mock object this introspection data belongs to.
        /// </summary>
        new T Object { get; }
    }
}