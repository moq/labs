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
		public static void AddMockBehavior(this IMocked mock, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            mock.Mock.Behaviors.Add(MockBehavior.Create(appliesTo, behavior));
        }

        /// <summary>
        /// Inserts a behavior into the mock behavior pipeline at the specified 
        /// index.
        /// </summary>
		public static void InsertMockBehavior(this IMocked mock, int index, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            mock.Mock.Behaviors.Insert(index, MockBehavior.Create(appliesTo, behavior));
        }

        /// <summary>
        /// Adds a behavior to a mock.
        /// </summary>
		public static void AddMockBehavior(this object mock, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            if (mock is IMocked mocked)
                mocked.Mock.Behaviors.Add(MockBehavior.Create(appliesTo, behavior));
            else
                throw new ArgumentException(nameof(mock));
        }

        /// <summary>
        /// Inserts a behavior into the mock behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static void InsertMockBehavior(this object mock, int index, Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
        {
            if (mock is IMocked mocked)
            {
                mocked.Mock.Behaviors.Insert(index, MockBehavior.Create(appliesTo, behavior));
            }
            else
                throw new ArgumentException(nameof(mock));
        }

        /// <summary>
        /// Adds a behavior to a mock.
        /// </summary>
		public static void AddMockBehavior(this object mock, IMockBehavior behavior)
        {
            if (mock is IMocked target)
                target.Mock.Behaviors.Add(behavior);
            else
                throw new ArgumentException(nameof(mock));
        }

        /// <summary>
        /// Inserts a behavior into the mock behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static void InsertMockBehavior(this object mock, int index, IMockBehavior behavior)
        {
            if (mock is IMocked target)
            {
                target.Mock.Behaviors.Insert(index, behavior);
            }
            else
                throw new ArgumentException(nameof(mock));
        }
    }
}
