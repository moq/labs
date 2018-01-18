using System.ComponentModel;
using System.Linq;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class MockedExtensions
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
        public static void SetBehavior(this IMocked mocked, MockBehavior behavior)
        {
            mocked.Mock.Behaviors.Clear();
            
            mocked.Mock.Behaviors.Add(new MockTrackingBehavior());
            mocked.Mock.Behaviors.Add(new EventBehavior());
            mocked.Mock.Behaviors.Add(new PropertyBehavior { SetterRequiresSetup = behavior == MockBehavior.Strict });
            mocked.Mock.Behaviors.Add(new DefaultEqualityBehavior());
            mocked.Mock.Behaviors.Add(new RecursiveMockBehavior());

            if (behavior == MockBehavior.Strict)
                mocked.Mock.Behaviors.Add(new StrictMockBehavior());
            else
                mocked.Mock.Behaviors.Add(new DefaultValueBehavior());

            mocked.Mock.State.Set(behavior);
            // Saves the initial set of behaviors, which allows resetting the mock.
            mocked.Mock.State.Set(mocked.Mock.Behaviors.ToArray());
        }

        public static MockBehavior? GetBehavior(this IMocked mocked)
            => mocked.Mock.State.TryGetValue<MockBehavior>(out var behavior) ? behavior : (MockBehavior?)null;
    }
}
