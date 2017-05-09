using System;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Usability functions for working with mocks.
    /// </summary>
    public static class MockExtensions
    {
        /// <summary>
        /// Adds a behavior to a mock.
        /// </summary>
		public static IMocked AddMockBehavior(this IMocked mock, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            mock.Mock.Behaviors.Add(MockBehavior.Create(appliesTo, behavior));
            return mock;
        }

        /// <summary>
        /// Inserts a behavior into the mock behavior pipeline at the specified 
        /// index.
        /// </summary>
		public static IMocked InsertMockBehavior(this IMocked mock, int index, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            mock.Mock.Behaviors.Insert(index, MockBehavior.Create(appliesTo, behavior));
            return mock;
        }

        /// <summary>
        /// Adds a behavior to a mock.
        /// </summary>
		public static IMock AddMockBehavior(this IMock mock, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            mock.Behaviors.Add(MockBehavior.Create(appliesTo, behavior));
            return mock;
        }

        /// <summary>
        /// Inserts a behavior into the mock behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static IMock InsertMockBehavior(this IMock mock, int index, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            mock.Behaviors.Insert(index, MockBehavior.Create(appliesTo, behavior));
            return mock;
        }

        /// <summary>
        /// Adds a behavior to a mock.
        /// </summary>
		public static TMock AddMockBehavior<TMock>(this TMock mock, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            // We can't just add a constraint to the method signature, because this is 
            // implemented internally for Moq.Sdk to consume.
            if (mock is IMocked mocked)
                mocked.Mock.Behaviors.Add(MockBehavior.Create(appliesTo, behavior));
            else
                throw new ArgumentException(nameof(mock));

            return mock;
        }

        /// <summary>
        /// Inserts a behavior into the mock behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static TMock InsertMockBehavior<TMock>(this TMock mock, int index, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            if (mock is IMocked mocked)
                mocked.Mock.Behaviors.Insert(index, MockBehavior.Create(appliesTo, behavior));
            else
                throw new ArgumentException(nameof(mock));

            return mock;
        }

        /// <summary>
        /// Adds a behavior to a mock.
        /// </summary>
        public static TMock AddMockBehavior<TMock>(this TMock mock, IMockBehavior behavior)
        {
            if (mock is IMocked target)
                target.Mock.Behaviors.Add(behavior);
            else
                throw new ArgumentException(nameof(mock));

            return mock;
        }

        /// <summary>
        /// Inserts a behavior into the mock behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static TMock InsertMockBehavior<TMock>(this TMock mock, int index, IMockBehavior behavior)
        {
            if (mock is IMocked target)
                target.Mock.Behaviors.Insert(index, behavior);
            else
                throw new ArgumentException(nameof(mock));

            return mock;
        }
    }
}
