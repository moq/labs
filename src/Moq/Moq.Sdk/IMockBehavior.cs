using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Represents a unit of behavior (such as returning a value, 
    /// invoking a callback or throwing an exception) that applies 
    /// to a mock when a given setup is matched (such as a particular 
    /// method being called with specific arguments).
    /// </summary>
    public interface IMockBehavior
    {
        /// <summary>
        /// Executes the behavior for the given invocation.
        /// </summary>
        /// <param name="invocation">The current method invocation.</param>
        /// <param name="next">Delegate to invoke the next behavior in the pipeline.</param>
        /// <returns>The result of the method invocation.</returns>
        IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next);
    }
}