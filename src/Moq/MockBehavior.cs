using System.ComponentModel;

namespace Moq
{
    /// <summary>
    /// Options to customize the behavior of the mock. 
    /// </summary>
    public enum MockBehavior
    {
        /// <summary>Obsolete</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Default = 0,
        /// <summary>
        /// Will never throw exceptions, returning default  
        /// values when necessary (null for reference types, 
        /// zero for value types and empty enumerables and arrays).
        /// </summary>
        Loose = 0,
        /// <summary>
        /// Causes the mock to always throw 
        /// an exception for invocations that don't have a 
        /// corresponding setup.
        /// </summary>
        Strict = 1,
    }
}
