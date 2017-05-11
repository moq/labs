using System.Collections.Generic;
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
        /// <summary>
        /// Sets the return value for a property or non-void method.
        /// </summary>
        public static void Returns<TResult>(this object target, TResult value)
        {
            var invocation = CallContext<IMethodInvocation>.GetData(nameof(IMethodInvocation));
            var mock = ((IMocked)invocation.Target).Mock;
            var filter = BuildFilter(invocation);

            mock.Invocations.Remove(invocation);
            mock.AddBehavior(filter.AppliesTo, (mi, next) => mi.CreateValueReturn(value, mi.Arguments));
        }

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static void Returns<TResult>(this object target, Func<TResult> value)
        {
            var invocation = CallContext<IMethodInvocation>.GetData(nameof(IMethodInvocation));
            var mock = ((IMocked)invocation.Target).Mock;
            var filter = BuildFilter(invocation);

            mock.Invocations.Remove(invocation);

            mock.AddBehavior(filter.AppliesTo, (mi, next) => mi.CreateValueReturn(value(), mi.Arguments));
        }

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static void Returns<T, TResult>(this object target, Func<T, TResult> value)
            => Returns(value, (mi, next)
                => mi.CreateValueReturn(value((T)mi.Arguments[0]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static void Returns<T1, T2, TResult>(this object target, Func<T1, T2, TResult> value)
            => Returns(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static void Returns<T1, T2, T3, TResult>(this object target, Func<T1, T2, T3, TResult> value)
            => Returns(value, (mi, next) 
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2]), mi.Arguments));

        static void Returns(Delegate value, InvokeBehavior behavior)
        {
            var invocation = CallContext<IMethodInvocation>.GetData(nameof(IMethodInvocation));
            EnsureCompatible(invocation, value);
            var mock = ((IMocked)invocation.Target).Mock;
            var filter = BuildFilter(invocation);

            mock.Invocations.Remove(invocation);
            mock.AddBehavior(filter.AppliesTo, behavior);
        }

        static void EnsureCompatible(IMethodInvocation invocation, Delegate callback)
        {
            var method = callback.GetMethodInfo();
            if (invocation.Arguments.Count != method.GetParameters().Length)
                throw new ArgumentException("Callback arguments do not match target invocation arguments.");

            // TODO: validate assignability
        }

        static SimpleBehaviorFilter BuildFilter(IMethodInvocation invocation)
        {
            var currentMatchers = CallContext<Queue<IArgumentMatcher>>.GetData(nameof(IArgumentMatcher), () => new Queue<IArgumentMatcher>());
            var finalMatchers = new List<IArgumentMatcher>();
            var parameters = invocation.MethodBase.GetParameters();

            for (var i = 0; i < invocation.Arguments.Count; i++)
            {
                var argument = invocation.Arguments[i];
                var parameter = parameters[i];

                if (object.Equals(argument, DefaultValue.For(parameter.ParameterType)) &&
                    currentMatchers.Count != 0 &&
                    parameter.ParameterType.GetTypeInfo().IsAssignableFrom(currentMatchers.Peek().ArgumentType.GetTypeInfo()))
                {
                    finalMatchers.Add(currentMatchers.Dequeue());
                }
                else
                {
                    finalMatchers.Add(new ValueMatcher(parameter.ParameterType, argument));
                }
            }

            return new SimpleBehaviorFilter(invocation, finalMatchers);
        }
    }
}
