using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Evaluates a list of <see cref="IArgumentMatcher"/>s against an 
    /// <see cref="IMethodInvocation"/> to determine if it satisfies the 
    /// requirements against the <see cref="IMethodInvocation"/> used 
    /// when the behavior was originally set up.
    /// </summary>
    public class InvocationFilterBehavior : IMockBehavior
    {
        InvokeBehavior behavior;

        /// <summary>
        /// Creates an instance of the behavior.
        /// </summary>
        /// <param name="filter">The filter that provides the implementation for <see cref="AppliesTo"/> method.</param>
        /// <param name="behavior">The actual behavior to execute when <see cref="Invoke"/> is executed.</param>
        public InvocationFilterBehavior(InvocationFilter filter, InvokeBehavior behavior, string name = null)
        {
            this.behavior = behavior;
            Filter = filter;
            Name = name;
        }

        public InvocationFilter Filter { get; }

        public string Name { get; }

        /// <summary>
        /// Evaluates the filter against an actual invocation.
        /// </summary>
        /// <param name="invocation">The actual invocation being performed on the mock.</param>
        /// <returns>Whether the filter applies to the given invocation.</returns>
        public bool AppliesTo(IMethodInvocation actualInvocation) => Filter.AppliesTo(actualInvocation);

        /// <summary>
        /// Invokes the configured behavior.
        /// </summary>
        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) => behavior(invocation, getNext);

        /// <summary>
        /// Provides a friendly representation of the filter. 
        /// </summary>
        public override string ToString() => Filter.ToString();
    }
}