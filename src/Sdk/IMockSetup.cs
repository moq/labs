using System.Collections;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// The configuration used to set up a <see cref="IMockBehavior" />.
    /// </summary>
    public interface IMockSetup : IStructuralEquatable
    {
        /// <summary>
        /// The <see cref="IMethodInvocation"/> used to set up the behavior.
        /// </summary>
        IMethodInvocation Invocation { get; }

        /// <summary>
        /// The <see cref="IArgumentMatcher"/>s used to set up the behavior.
        /// </summary>
        IArgumentMatcher[] Matchers { get; }

        /// <summary>
        /// Tests whether the current setup applies to an actual invocation.
        /// </summary>
        /// <param name="actualInvocation">An actual invocation performed 
        /// on the mock.
        /// </param>
        bool AppliesTo(IMethodInvocation actualInvocation);
    }
}