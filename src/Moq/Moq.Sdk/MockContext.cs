using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Provides information about the current mock call 
    /// context.
    /// </summary>
    public static class MockContext
    {
        /// <summary>
        /// The most recent invocation performed on the mock, tracked 
        /// by the <see cref="MockTrackingBehavior"/>.
        /// </summary>
        public static IMethodInvocation CurrentInvocation => CallContext<IMethodInvocation>.GetData();

        /// <summary>
        /// The last invocation on the mock, turned into an <see cref="IMockSetup"/> 
        /// ready for use together with the <see cref="IMock.BehaviorFor(IMockSetup)"/> 
        /// method to locate the matching <see cref="IMockBehavior"/> to add new behaviors
        /// when a matching invocation is performed.
        /// </summary>
        /// <remarks>
        /// This property is also tracked and populated by the 
        /// <see cref="MockTrackingBehavior"/>.
        /// </remarks>
        public static IMockSetup CurrentSetup => CallContext<IMockSetup>.GetData();
    }
}
