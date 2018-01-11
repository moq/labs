using System.Collections.Generic;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Provides introspection information about a mock.
    /// </summary>
    public interface IMock : IStunt
    {
        /// <summary>
        /// Returns a <see cref="IMockBehavior"/> for the given <see cref="IMockSetup"/>.
        /// </summary>
        /// <param name="setup">The setup that matches the returned <see cref="IMockBehavior.Setup"/>.</param>
        IMockBehavior BehaviorFor(IMockSetup setup);

        /// <summary>
        /// Invocations performed on the mock so far.
        /// </summary>
        IList<IMethodInvocation> Invocations { get; }

        /// <summary>
        /// Arbitrary state associated with a mock instance.
        /// </summary>
        MockState State { get; }
    }
}
