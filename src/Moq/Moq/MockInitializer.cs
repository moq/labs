using System.ComponentModel;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Provides the <see cref="Initialize"/> method for configuring the initial 
    /// set of behaviors for a given <see cref="MockBehavior"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MockInitializer
    {
        /// <summary>
        /// Clears any existing behavior (including any setups) and 
        /// adds the necessary behaviors to the <paramref name="mocked"/> so 
        /// that it behaves as specified by the <paramref name="behavior"/> 
        /// enumeration.
        /// </summary>
        /// <remarks>
        /// This method can be used by custom mocks to ensure they have the 
        /// same default behaviors as a mock created using <c>Mock.Of{T}</c>.
        /// </remarks>
        public static void Initialize(this IMocked mocked, MockBehavior behavior = MockBehavior.Loose)
        {
            var mock = mocked.Mock;
            mock.Behaviors.Clear();

            mocked.AsMoq().Behavior = behavior;

            mock.Behaviors.Add(new SetupScopeBehavior());
            mock.Behaviors.Add(new MockContextBehavior());
            mock.Behaviors.Add(new ConfigurePipelineBehavior());
            mock.Behaviors.Add(new MockRecordingBehavior());
            mock.Behaviors.Add(new EventBehavior());
            mock.Behaviors.Add(new PropertyBehavior { SetterRequiresSetup = behavior == MockBehavior.Strict });
            mock.Behaviors.Add(new DefaultEqualityBehavior());
            mock.Behaviors.Add(new RecursiveMockBehavior());
            mock.Behaviors.Add(new CallBaseBehavior());

            // Dynamically configured by the ConfigurePipelineBehavior.
            mock.Behaviors.Add(new StrictMockBehavior());

            var defaultValue = mock.State.GetOrAdd(() => new DefaultValueProvider());
            mock.Behaviors.Add(new DefaultValueBehavior(defaultValue));
            mock.State.Set(defaultValue);
        }
    }
}
