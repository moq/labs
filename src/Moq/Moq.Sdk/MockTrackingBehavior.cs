using System;
using Stunts;
using Moq.Sdk.Properties;

namespace Moq.Sdk
{
    /// <summary>
    /// Core behavior that allows tracking invocations and 
    /// building setups from them. 
    /// <para>
    /// Populates <see cref="IMock.LastSetup"/> for use from 
    /// fluent extension method APIs, the <see cref="CallContext{IMethodInvocation}"/> 
    /// as well as the <see cref="CallContext{IMockSetup}"/>.
    /// </para>
    /// </summary>
    public class MockTrackingBehavior : IStuntBehavior
    {
        public bool AppliesTo(IMethodInvocation invocation) => true;

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            var mock = invocation.Target is IMocked mocked ?
                mocked.Mock : throw new ArgumentException(Resources.TargetNotMock);

            // Allows subsequent extension methods on the fluent API to retrieve the 
            // current invocation being performed via the MockContext.
            CallContext<IMethodInvocation>.SetData(invocation);

            // Determines the current setup according to contextual 
            // matchers that may have been pushed to the MockSetup context. 
            // Allows subsequent extension methods on the fluent API to retrieve the 
            // current setup being performed via the MockContext.
            CallContext<IMockSetup>.SetData(MockSetup.Freeze(invocation));

            mock.Invocations.Add(invocation);

            return getNext().Invoke(invocation, getNext);
        }
    }
}