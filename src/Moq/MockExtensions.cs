using Moq.Proxy;
using Moq.Sdk;
using System.Reflection;
using System.ComponentModel;
using System;

namespace Moq
{
    /// <summary>
    /// Extensions for configuring mocks.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class MockExtensions
    {
        public static TResult Callback<TResult>(this TResult target, Action callback)
        {
            var setup = MockSetup.Current;
            if (setup != null)
            {
                var mock = ((IMocked)setup.Invocation.Target).Mock;

                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.BehaviorFor(setup);
                behavior.Behaviors.Add(new InvocationBehavior(
                    (mi, next) =>
                    {
                        callback();
                        return next()(mi, next);
                    },
                    "Callback")
               );
            }

            return target;
        }

        /// <summary>
        /// Sets the return value for a property or non-void method.
        /// </summary>
        public static TResult Returns<TResult>(this object target, TResult value)
        {
            var setup = MockSetup.Current;
            if (setup != null)
            {
                var mock = ((IMocked)setup.Invocation.Target).Mock;
                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.BehaviorFor(setup);
                behavior.Behaviors.Insert(0, new InvocationBehavior(
                    (mi, next) => mi.CreateValueReturn(value, mi.Arguments),
                    "Returns")
                );
            }

            return default(TResult);
        }

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<TResult>(this object target, Func<TResult> value)
        {
            var setup = MockSetup.Current;
            if (setup != null)
            {
                var mock = ((IMocked)setup.Invocation.Target).Mock;
                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.BehaviorFor(setup);
                behavior.Behaviors.Insert(0, new InvocationBehavior(
                    (mi, next) => mi.CreateValueReturn(value(), mi.Arguments),
                    "Returns")
               );
            }

            return default(TResult);
        }

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T, TResult>(this object target, Func<T, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T)mi.Arguments[0]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, TResult>(this object target, Func<T1, T2, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, TResult>(this object target, Func<T1, T2, T3, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2]), mi.Arguments));

        static TResult Returns<TResult>(Delegate value, InvokeBehavior behavior)
        {
            var setup = MockSetup.Current;
            if (setup != null)
            {
                EnsureCompatible(setup.Invocation, value);
                var mock = ((IMocked)setup.Invocation.Target).Mock;

                mock.Invocations.Remove(setup.Invocation);
                var mockBehavior = mock.BehaviorFor(setup);

                mockBehavior.Behaviors.Add(new InvocationBehavior(behavior, "Returns"));
            }

            return default(TResult);
        }

        static void EnsureCompatible(IMethodInvocation invocation, Delegate callback)
        {
            var method = callback.GetMethodInfo();
            if (invocation.Arguments.Count != method.GetParameters().Length)
                throw new ArgumentException("Callback arguments do not match target invocation arguments.");

            // TODO: validate assignability
        }
    }
}
