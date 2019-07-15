using System;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// A verification failed to match a mock's invocations 
    /// against a given <see cref="Setup"/> setup.
    /// </summary>
    public class VerifyException : MockException
    {
        /// <summary>
        /// Initializes the exception with the target 
        /// mock and setup that failed to match invocations.
        /// </summary>
        public VerifyException(IMock mock, IMockSetup setup)
            : base(setup.ToString())
        {
            Mock = mock;
            Setup = setup;
        }

        /// <summary>
        /// The expected setup that should have matched the 
        /// invocations on the mock.
        /// </summary>
        public IMockSetup Setup { get; }

        /// <summary>
        /// The mock that was tested against the given setup.
        /// </summary>
        public IMock Mock { get; }
    }

    /// <summary>
    /// A verification failed to match a mock's invocations 
    /// against a given expected <see cref="IMockSetup"/> setup.
    /// </summary>
    /// <typeparam name="T">The type of the mocked instance.</typeparam>
    public class VerifyException<T> : VerifyException
    {
        /// <summary>
        /// Initializes the exception with the target 
        /// mock and setup that failed to match invocations.
        /// </summary>
        public VerifyException(IMock<T> mock, IMockSetup setup)
            : base(mock, setup)
        {
            Mock = mock;
        }

        /// <summary>
        /// The mock that was tested against the given setup.
        /// </summary>
        public new IMock<T> Mock { get; }
    }
}
