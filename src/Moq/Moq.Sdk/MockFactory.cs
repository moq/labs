using System;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Allows accessing the default <see cref="IMockFactory"/> to use to 
    /// create mocks.
    /// </summary>
    public static class MockFactory
    {
        /// <summary>
        /// Gets or sets the default <see cref="IMockFactory"/> to use 
        /// to create mocks.
        /// </summary>
        public static IMockFactory Default { get; set; }
   }
}