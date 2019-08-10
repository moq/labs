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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MockExtensions
    {
        /// <summary>
        /// Gets the introspection information for a mocked object instance.
        /// </summary>
        public static IMock<T> AsMock<T>(this T instance) where T : class
            => (instance as IMocked)?.Mock.As(instance) ?? throw new ArgumentException(Strings.TargetNotMock, nameof(instance));

        /// <summary>
        /// Clones a mock by creating a new instance of the <see cref="IMock.Object"/> 
        /// from <paramref name="mock"/> and copying its behaviors, invocations and state.
        /// </summary>
        public static IMock<T> Clone<T>(this IMock<T> mock) where T : class
        {
            if (!mock.State.TryGetValue<object[]>(".ctor", out var ctor))
                throw new ArgumentException("No constructor state found for cloning.");

            // TODO: THIS DOESN'T WORK WITH DYNAMIC PROXIES, SINCE WE'RE MISSING THE INTERCEPTORS
            // This is what it looks like in a DP: public BaseWithCtorProxy(IInterceptor[] interceptorArray, string value) : base(value)
            // So we need to persist the interceptors as part of the ctor array, maybe?
            var clone = ((IMocked)Activator.CreateInstance(mock.Object.GetType(), ctor)).Mock;
            clone.State = mock.State.Clone();

            clone.Behaviors.Clear();
            foreach (var behavior in mock.Behaviors)
            {
                clone.Behaviors.Add(behavior);
            }

            clone.Invocations.Clear();
            foreach (var invocation in mock.Invocations)
            {
                clone.Invocations.Add(invocation);
            }

            return ((T)clone.Object).AsMock();
        }

        /// <summary>
        /// Gets the invocations performed on the mock so far that match the given 
        /// setup lambda.
        /// </summary>
        public static IEnumerable<IMethodInvocation> InvocationsFor<T>(this IMock<T> mock, Action<T> action) where T : class
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
        public static IEnumerable<IMethodInvocation> InvocationsFor<T, TResult>(this IMock<T> mock, Func<T, TResult> function) where T : class
        {
            using (new SetupScope())
            {
                function(mock.Object);
                var setup = MockContext.CurrentSetup;
                return mock.Invocations.Where(x => setup.AppliesTo(x));
            }
        }

        static IMock<T> As<T>(this IMock mock, T target) where T : class => mock == null ? null : new Mock<T>(mock, target);

        class Mock<T> : IMock<T> where T : class
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

            public StateBag State
            {
                get => mock.State;
                set => mock.State = value;
            }

            public IEnumerable<IMockBehaviorPipeline> Setups => mock.Setups;

            public ObservableCollection<IStuntBehavior> Behaviors => mock.Behaviors;

            public IMockBehaviorPipeline GetPipeline(IMockSetup setup) => mock.GetPipeline(setup);
        }
    }
}
