using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using Stunts;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MockTrackingBehaviorTests
    {
        [Fact]
        public void SetsCurrentInvocationAndSetup()
        {
            var target = new TrackingMock();
            var invocation = new MethodInvocation(target, typeof(TrackingMock).GetMethod(nameof(TrackingMock.Do)));
            var tracking = new MockTrackingBehavior();

            Assert.NotNull(tracking.Execute(invocation, () => (m, n) => m.CreateValueReturn(null)));

            Assert.Same(invocation, MockContext.CurrentInvocation);
            Assert.NotNull(MockContext.CurrentSetup);
            Assert.True(MockContext.CurrentSetup.AppliesTo(invocation));
        }

        [Fact]
        public void RecordsInvocation()
        {
            var target = new TrackingMock();
            var invocation = new MethodInvocation(target, typeof(TrackingMock).GetMethod(nameof(TrackingMock.Do)));
            var tracking = new MockTrackingBehavior();

            Assert.NotNull(tracking.Execute(invocation, () => (m, n) => m.CreateValueReturn(null)));

            Assert.Single(target.Mock.Invocations);
        }

        [Fact]
        public void SkipInvocationRecordingIfSetupScopeActive()
        {
            var target = new TrackingMock();
            var invocation = new MethodInvocation(target, typeof(TrackingMock).GetMethod(nameof(TrackingMock.Do)));
            var tracking = new MockTrackingBehavior();

            using (new SetupScope())
            {
                Assert.NotNull(tracking.Execute(invocation, () => (m, n) => m.CreateValueReturn(null)));
            }

            Assert.Empty(target.Mock.Invocations);
        }


        class TrackingMock : FakeMock
        {
            public void Do() => Pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        }
    }
}
