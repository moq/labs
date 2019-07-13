using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Provides configuration information for a mock.
    /// </summary>
    public interface IMoq : IMock, IFluentInterface
    {
        /// <summary>
        /// Gets the <see cref="MockBehavior"/> of the mock.
        /// </summary>
        MockBehavior Behavior { get; set; }

        /// <summary>
        /// Gets the default value behavior of the mock. 
        /// Only available for <see cref="MockBehavior.Loose"/> mocks.
        /// </summary>
        DefaultValueProvider DefaultValue { get; set; }
    }
}
