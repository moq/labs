using System;

namespace Moq.Sdk
{
    /// <summary>
    /// Base exception thrown by mocking APIs.
    /// </summary>
    public abstract class MockException : Exception
    {
        /// <summary>
        /// Initializes the exception with no message.
        /// </summary>
        protected MockException() { }

        /// <summary>
        /// Initializes the exception with the specified message.
        /// </summary>
        /// <param name="message"></param>
        protected MockException(string message)
            : base(message) { }
    }
}
