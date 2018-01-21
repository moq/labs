using System;
using System.ComponentModel;
using Moq.Sdk;
using Moq.Sdk.Properties;

namespace Moq
{
    /// <summary>
    /// Allows accessing the <see cref="IMock"/> introspection information 
    /// for a mocked object instance.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class Extensions
    {
        /// <summary>
        /// Gets the introspection information for a mocked object instance.
        /// </summary>
        public static IMock GetMock(this object mock) 
            => (mock as IMocked)?.Mock ?? throw new ArgumentException(Strings.TargetNotMock, nameof(mock));
    }
}
