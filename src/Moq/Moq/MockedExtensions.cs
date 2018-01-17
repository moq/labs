using System.ComponentModel;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class MockedExtensions
    {
        /// <summary>
        /// Clears any existing behavior (including any setups) and 
        /// adds the necessary behaviors to the <paramref name="mock"/> so 
        /// that it behaves as specified by the <paramref name="behavior"/> 
        /// enumeration.
        /// </summary>
        /// <remarks>
        /// This method can be used by custom mocks to ensure they have the 
        /// same default behaviors as a mock created using <c>Mock.Of{T}</c>.
        /// </remarks>
        public static void SetBehavior(this IMocked mock, MockBehavior behavior)
        {
            mock.Mock.Behaviors.Clear();

            mock.Mock.Behaviors.Add(new MockTrackingBehavior());
            mock.Mock.Behaviors.Add(new EventBehavior());
            mock.Mock.Behaviors.Add(new PropertyBehavior { SetterRequiresSetup = behavior == MockBehavior.Strict });
            mock.Mock.Behaviors.Add(new DefaultEqualityBehavior());

            if (behavior == MockBehavior.Strict)
                mock.Mock.Behaviors.Add(new StrictMockBehavior());
            else
                mock.Mock.Behaviors.Add(new DefaultValueBehavior());
        }
    }
}
