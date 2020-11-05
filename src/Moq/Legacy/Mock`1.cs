#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq.Sdk;

namespace Moq
{
    /// <summary>Supports the legacy API.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Mock<T> where T : class
    {
        static Mock()
        {
            if (MockFactory.Default == MockFactory.NotImplemented)
                MockFactory.Default = new DynamicMockFactory();
        }

        readonly T target;
        readonly IMock<T> mock;
        readonly MockBehavior behavior;

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MockGenerator]
        public Mock() : this(Assembly.GetCallingAssembly(), MockBehavior.Loose)
        {
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MockGenerator]
        public Mock(params object[] args) : this(Assembly.GetCallingAssembly(), MockBehavior.Loose, args)
        {
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MockGenerator]
        public Mock(MockBehavior behavior, params object[] args) : this(Assembly.GetCallingAssembly(), behavior, args)
        {
        }

        protected Mock(Assembly mocksAssembly, MockBehavior behavior, params object[] args)
        {
            var instance = MockFactory.Default.CreateMock(mocksAssembly, typeof(T), new Type[0], args);
            var mocked = instance is MulticastDelegate @delegate ?
                (IMocked)@delegate.Target : (IMocked)instance;

            mocked.Initialize(behavior);

            target = (T)instance;
            mock = target.AsMock();
        }

        Mock(T target, IMock<T> mock, MockBehavior behavior)
        {
            this.target = target;
            this.mock = mock;
            this.behavior = behavior;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public T Object => target;

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool CallBase
        {
            get => mock.AsMoq().CallBase;
            set => mock.AsMoq().CallBase = value;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Mock<TInterface> As<TInterface>() where TInterface : class
        {
            if (target is not TInterface iface)
                throw new InvalidOperationException(ThisAssembly.Strings.Legacy.AsInterfaceNotImplemented);

            return new Mock<TInterface>(iface, iface.AsMock(), behavior);
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetupAllProperties() { } // TODO

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<T, TResult> Setup<TResult>(Expression<Func<T, TResult>> expression)
            => new MockSetup<T, TResult>(target, mock, expression.Compile());

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<T, TResult> SetupGet<TResult>(Expression<Func<T, TResult>> expression)
            => new MockSetup<T, TResult>(target, mock, expression.Compile());

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<T> Setup(Expression<Action<T>> expression)
            => new MockSetup<T>(target, mock, expression.Compile());

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void VerifyAll()
        {
            var setups = mock.Setups
                .Where(setup => !Sdk.Times.AtLeastOnce.Validate(mock.Invocations.Where(x => setup.AppliesTo(x)).Count()))
                .Select(setup => setup.Setup)
                .ToArray();

            if (setups.Length > 0)
                throw new VerifyException(mock, setups);
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Verify() => Moq.Verify.Called(target);

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Verify(Expression<Action<T>> expression)
            => Moq.Verify.Called(() => expression.Compile().Invoke(target));

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Verify(Expression<Action<T>> expression, Times times, string? message = null)
            => Moq.Verify.CalledImpl(() => expression.Compile().Invoke(target), times.ToSdk(), message);

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Verify(Expression<Action<T>> expression, Func<Times> times, string? message = null)
            => Moq.Verify.CalledImpl(() => expression.Compile().Invoke(target), times().ToSdk(), message);

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Verify<TResult>(Expression<Func<T, TResult>> expression)
            => Moq.Verify.Called(() => expression.Compile().Invoke(target));

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Verify<TResult>(Expression<Func<T, TResult>> expression, Times times, string? message = null)
            => Moq.Verify.CalledImpl(() => expression.Compile().Invoke(target), times.ToSdk(), message);

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Verify<TResult>(Expression<Func<T, TResult>> expression, Func<Times> times, string? message = null)
            => Moq.Verify.CalledImpl(() => expression.Compile().Invoke(target), times().ToSdk(), message);
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member