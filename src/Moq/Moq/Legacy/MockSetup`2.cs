using System;
using System.ComponentModel;
using Moq.Sdk;

namespace Moq
{
    /// <summary>Supports the legacy API.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MockSetup<TTarget, TResult> where TTarget : class
    {
        private readonly TTarget target;
        private readonly IMock<TTarget> mock;
        private readonly Func<TTarget, TResult> function;

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup(TTarget target, IMock<TTarget> mock, Func<TTarget, TResult> function)
        {
            this.target = target;
            this.mock = mock;
            this.function = function;
            target.Setup(function);
        }

        #region Callback

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T1, T2>(Action<T1, T2> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback<T>(Action<T> action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Callback(Action action)
        {
            target.Setup(action).Callback(action);
            return this;
        }

        #endregion

        #region Returns

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3, T4>(Func<T1, T2, T3, T4, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2, T3>(Func<T1, T2, T3, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T1, T2>(Func<T1, T2, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns<T>(Func<T, TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns(Func<TResult> valueFunction)
        {
            target.Setup(function).Returns(valueFunction);
            return this;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Returns(TResult value)
        {
            target.Setup(function).Returns(value);
            return this;
        }

        #endregion

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Verifiable() => target.Setup(function).Verifiable();

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MockSetup<TTarget, TResult> Throws(Exception exception)
        {
            target.Setup(function)!.Throws(exception);
            return this;
        }
    }
}