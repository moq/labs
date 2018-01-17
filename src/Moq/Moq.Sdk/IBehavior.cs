using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Represents a unit of behavior (such as returning a value, 
    /// invoking a callback or throwing an exception) that applies 
    /// to a mock when a given setup is matched (such as a particular 
    /// method being called with specific arguments).
    /// </summary>
    public interface IBehavior
    {
        /// <summary>
        /// The delegate that implements the behavior.
        /// </summary>
        InvokeBehavior Invoke { get; }
    }
}