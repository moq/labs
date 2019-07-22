﻿using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// A verification failed to match a mock's invocations 
    /// against the given <see cref="Setups"/> setup(s).
    /// </summary>
    public class VerifyException : MockException
    {
        /// <summary>
        /// Initializes the exception with the target 
        /// mock and setup(s) that failed to match invocations.
        /// </summary>
        public VerifyException(IMock mock, IMockSetup setup, string message = null)
            : this(mock, new[] { setup }, message)
        {
        }

        /// <summary>
        /// Initializes the exception with the target 
        /// mock and setup(s) that failed to match invocations.
        /// </summary>
        public VerifyException(IMock mock, IMockSetup[] setups, string message = null)
            : base(message)
        {
            Mock = mock;
            Setups = setups;
        }

        /// <summary>
        /// The expected setups that should have matched the invocations on the mock 
        /// but didn't.
        /// </summary>
        public IMockSetup[] Setups { get; }

        /// <summary>
        /// The mock that was tested.
        /// </summary>
        public IMock Mock { get; }
    }
}
