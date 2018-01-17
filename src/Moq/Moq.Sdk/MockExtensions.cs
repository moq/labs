using System;
using Moq.Sdk.Properties;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Usability functions for working with mocks.
    /// </summary>
    public static class MockExtensions
    {
        /// <summary>
        /// Adds a behavior to a mock for the current <see cref="MockContext.CurrentSetup"/> setup.
        /// </summary>
        public static IMock AddBehavior(this IMock mock, InvokeBehavior behavior, Lazy<string> displayName)
        {
            mock
                .BehaviorFor(MockContext.CurrentSetup ?? throw new InvalidOperationException(Strings.NoCurrentSetup))
                .Behaviors
                .Add(new Behavior(behavior, displayName));
            
            return mock;
        }

        /// <summary>
        /// Inserts a behavior into the mock behavior pipeline at the specified 
        /// index for the current <see cref="MockContext.CurrentSetup"/> setup.
        /// </summary>
        public static IMock InsertBehavior(this IMock mock, int index, InvokeBehavior behavior, Lazy<string> displayName)
        {
            mock
                .BehaviorFor(MockContext.CurrentSetup ?? throw new InvalidOperationException(Strings.NoCurrentSetup))
                .Behaviors
                .Insert(index, new Behavior(behavior, displayName));

            return mock;
        }

        /// <summary>
        /// Adds a mock behavior to a mock.
        /// </summary>
        public static TMock AddBehavior<TMock>(this TMock mock, IMockBehavior behavior)
        {
            if (mock is IMocked mocked)
                mocked.Mock.Behaviors.Add(behavior);
            else if (mock is IMock m)
                m.Behaviors.Add(behavior);
            else
                throw new ArgumentException(Strings.TargetNotMock, nameof(mock));

            return mock;
        }

        /// <summary>
        /// Inserts a mock behavior into the mock behavior pipeline at the specified index.
        /// </summary>
        public static TMock InsertBehavior<TMock>(this TMock mock, int index, IMockBehavior behavior)
        {
            if (mock is IMocked mocked)
                mocked.Mock.Behaviors.Insert(index, behavior);
            else if (mock is IMock m)
                m.Behaviors.Insert(index, behavior);
            else
                throw new ArgumentException(Strings.TargetNotMock, nameof(mock));

            return mock;
        }
    }
}
