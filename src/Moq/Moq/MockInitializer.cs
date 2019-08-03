using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
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
            mocked.Mock.Behaviors.Clear();

            mocked.AsMoq().Behavior = behavior;

            mocked.Mock.Behaviors.Add(new SetupScopeBehavior());
            mocked.Mock.Behaviors.Add(new MockContextBehavior());
            mocked.Mock.Behaviors.Add(new ConfigurePipelineBehavior());
            mocked.Mock.Behaviors.Add(new MockRecordingBehavior());
            mocked.Mock.Behaviors.Add(new EventBehavior());
            mocked.Mock.Behaviors.Add(new PropertyBehavior { SetterRequiresSetup = behavior == MockBehavior.Strict });
            mocked.Mock.Behaviors.Add(new DefaultEqualityBehavior());
            mocked.Mock.Behaviors.Add(new RecursiveMockBehavior());
            mocked.Mock.Behaviors.Add(new CallBaseBehavior());

            // Dynamically configured by the ConfigurePipelineBehavior.
            mocked.Mock.Behaviors.Add(new StrictMockBehavior());

            var defaultValue = mocked.Mock.State.GetOrAdd(() => new DefaultValueProvider());
            mocked.Mock.Behaviors.Add(new DefaultValueBehavior(defaultValue));
            mocked.Mock.State.Set(defaultValue);
        }
    }
}
