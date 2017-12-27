using System;
using Stunts;
using Moq.Sdk.Properties;

namespace Moq.Sdk
{
    public class MockTrackingBehavior : IStuntBehavior
    {
        /// <summary>
        /// Optional boolean state value in <see cref="IMock.State"/> that 
        /// can enable/disable mock invocation tracking (for example, to 
        /// disable tracking while invoking a set of setup methods).
        /// </summary>
        public const string SkipTrackingState = nameof(SkipTrackingState);

        public bool AppliesTo(IMethodInvocation invocation) => true;

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            var mock = invocation.Target is IMocked mocked ?
                mocked.Mock : throw new ArgumentException(Resources.TargetNotMocked);

            var skipTracking = mock.State.TryGetValue<bool?>(SkipTrackingState, out var state) && state.GetValueOrDefault();

            if (!skipTracking)
            {
                // Allows subsequent extension methods on the fluent API to retrieve the 
                // current invocation being performed.
                CallContext<IMethodInvocation>.SetData(invocation);

                // Determines the current setup according to contextual 
                // matchers that may have been pushed to the MockSetup context. 
                // Allows subsequent extension methods on the fluent API to retrieve the 
                // current setup being performed.
                var setup = MockSetup.Freeze(invocation);
                CallContext<IMockSetup>.SetData(setup);

                mock.Invocations.Add(invocation);
                if (mock is MockInfo info)
                    info.LastSetup = setup;
            }

            return getNext().Invoke(invocation, getNext);
        }
    }
}