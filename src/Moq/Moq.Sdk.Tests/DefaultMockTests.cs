using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using Stunts;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class DefaultMockTests
    {
        [Fact]
        public void ThrowsIfNullStunt()
            => Assert.Throws<ArgumentNullException>(() => new DefaultMock(null));

        [Fact]
        public void AddsMockTrackingBehavior()
        {
            var mock = new DefaultMock(new FakeStunt());

            Assert.Collection(mock.Behaviors, x => Assert.IsType<MockTrackingBehavior>(x));
        }

        [Fact]
        public void PreventsDuplicateMockTrackingBehavior()
        {
            var mock = new DefaultMock(new FakeStunt());

            Assert.Throws<InvalidOperationException>(() => mock.Behaviors.Add(new MockTrackingBehavior()));
        }

        [Fact]
        public void TrackMockBehaviors()
        {
            var stunt = new FakeStunt();
            // Forces initialization of the default mock.
            Assert.NotNull(stunt.Mock);

            var setup = new MockSetup(
                new MethodInvocation(stunt, typeof(FakeStunt).GetMethod("Do")),
                Array.Empty<IArgumentMatcher>());

            var behavior = new MockBehaviorPipeline(setup);

            stunt.AddBehavior(behavior);
            stunt.AddBehavior(new DelegateStuntBehavior((m, n) => n().Invoke(m, n)));
            Assert.Equal(3, stunt.Behaviors.Count);

            Assert.Single(stunt.Mock.Setups);
            Assert.Same(behavior, stunt.Mock.GetPipeline(setup));

            stunt.Behaviors.Remove(behavior);

            Assert.Equal(2, stunt.Behaviors.Count);
            Assert.Empty(stunt.Mock.Setups);
        }

        [Fact]
        public void AddPipelineForSetupIfMissing()
        {
            var stunt = new FakeStunt();
            var setup = new MockSetup(
                new MethodInvocation(stunt, typeof(FakeStunt).GetMethod("Do")),
                Array.Empty<IArgumentMatcher>());

            var behavior = stunt.Mock.GetPipeline(setup);

            Assert.NotNull(behavior);
            Assert.Equal(2, stunt.Behaviors.Count);
            Assert.Single(stunt.Mock.Setups);
        }

        [Fact]
        public void TracksTargetObject()
        {
            var stunt = new FakeStunt();
            Assert.Same(stunt, stunt.Mock.Object);
        }

        [Fact]
        public void InitializesState()
            => Assert.NotNull(new FakeStunt().Mock.State);

        class FakeStunt : IStunt, IMocked
        {
            BehaviorPipeline pipeline = new BehaviorPipeline();
            DefaultMock mock;

            public ObservableCollection<IStuntBehavior> Behaviors => pipeline.Behaviors;

            public IMock Mock => LazyInitializer.EnsureInitialized(ref mock, () => new DefaultMock(this));

            public void Do() => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        }
    }
}
