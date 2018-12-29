using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Moq.Sdk.Properties;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Usability functions for working with mocks.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class MockExtensions
    {
        /// <summary>
        /// Adds a behavior to a mock for the current <see cref="MockContext.CurrentSetup"/> setup.
        /// </summary>
        public static IMock AddBehavior(this IMock mock, ExecuteDelegate behavior, Lazy<string> displayName)
        {
            mock
                .GetPipeline(MockContext.CurrentSetup ?? throw new InvalidOperationException(Strings.NoCurrentSetup))
                .Behaviors
                .Add(MockBehavior.Create(behavior, displayName));
            
            return mock;
        }

        /// <summary>
        /// Inserts a behavior into the mock behavior pipeline at the specified 
        /// index for the current <see cref="MockContext.CurrentSetup"/> setup.
        /// </summary>
        public static IMock InsertBehavior(this IMock mock, int index, ExecuteDelegate behavior, Lazy<string> displayName)
        {
            mock
                .GetPipeline(MockContext.CurrentSetup ?? throw new InvalidOperationException(Strings.NoCurrentSetup))
                .Behaviors
                .Insert(index, MockBehavior.Create(behavior, displayName));

            return mock;
        }

        /// <summary>
        /// Adds a mock behavior to a mock.
        /// </summary>
        public static TMock AddBehavior<TMock>(this TMock mock, IMockBehaviorPipeline behavior)
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
        public static TMock InsertBehavior<TMock>(this TMock mock, int index, IMockBehaviorPipeline behavior)
        {
            if (mock is IMocked mocked)
                mocked.Mock.Behaviors.Insert(index, behavior);
            else if (mock is IMock m)
                m.Behaviors.Insert(index, behavior);
            else
                throw new ArgumentException(Strings.TargetNotMock, nameof(mock));

            return mock;
        }

        /// <summary>
        /// Gets the introspection information for a mocked object instance.
        /// </summary>
        public static IMock<T> AsMock<T>(this T instance)
            => (instance as IMocked)?.Mock.As(instance) ?? throw new ArgumentException(Strings.TargetNotMock, nameof(instance));

        /// <summary>
        /// Gets the invocations performed on the mock so far that match the given 
        /// setup lambda.
        /// </summary>
        public static IEnumerable<IMethodInvocation> InvocationsFor<T>(this IMock<T> mock, Action<T> action)
        {
            using (new SetupScope())
            {
                action(mock.Object);
                var setup = MockContext.CurrentSetup;
                return mock.Invocations.Where(x => setup.AppliesTo(x));
            }
        }

        /// <summary>
        /// Gets the invocations performed on the mock so far that match the given 
        /// setup lambda.
        /// </summary>
        public static IEnumerable<IMethodInvocation> InvocationsFor<T, TResult>(this IMock<T> mock, Func<T, TResult> function)
        {
            using (new SetupScope())
            {
                function(mock.Object);
                var setup = MockContext.CurrentSetup;
                return mock.Invocations.Where(x => setup.AppliesTo(x));
            }
        }

        static IMock<T> As<T>(this IMock mock, T target) => mock == null ? null : new Mock<T>(mock, target);

        class Mock<T> : IMock<T>
        {
            IMock mock;

            public Mock(IMock mock, T target)
            {
                this.mock = mock;
                Object = target;
            }

            public T Object { get; }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object IMock.Object => mock.Object;

            public IList<IMethodInvocation> Invocations => mock.Invocations;

            public MockState State => mock.State;

            public IEnumerable<IMockBehaviorPipeline> Setups => mock.Setups;

            public ObservableCollection<IStuntBehavior> Behaviors => mock.Behaviors;

            public IMockBehaviorPipeline GetPipeline(IMockSetup setup) => mock.GetPipeline(setup);
        }
    }
}
