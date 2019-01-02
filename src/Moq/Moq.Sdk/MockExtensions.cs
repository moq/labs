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

            public ICollection<IMethodInvocation> Invocations => mock.Invocations;

            public MockState State => mock.State;

            public IEnumerable<IMockBehaviorPipeline> Setups => mock.Setups;

            public ObservableCollection<IStuntBehavior> Behaviors => mock.Behaviors;

            public IMockBehaviorPipeline GetPipeline(IMockSetup setup) => mock.GetPipeline(setup);
        }
    }
}
