using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Allows pushing argument matchers that can be converted into a 
    /// filter by either calling <see cref="Pop"/> (which removes the 
    /// matchers accumulated so far) or <see cref="Peek"/> (which just 
    /// builds up the filter with the current matchers, but does not 
    /// remove them from the current context).
    /// </summary>
    public static class Matchers
    {
        static AsyncLocal<InvocationFilter> lastFilter = new AsyncLocal<InvocationFilter>();

        /// <summary>
        /// Gets a filter that can be used as the <see cref="IMockBehavior.AppliesTo(IMethodInvocation)"/> 
        /// implementation given the current context and setup <paramref name="invocation"/>.
        /// </summary>
        public static InvocationFilter AppliesTo(IMethodInvocation invocation)
            => lastFilter.Value ?? throw new InvalidOperationException();

        /// <summary>
        /// Pushes an argument matcher in the current <see cref="CallContext{Queue{IArgumentMatcher}}"/>.
        /// </summary>
        public static void Push(IArgumentMatcher matcher) => Push<object>(matcher);

        /// <summary>
        /// Pushes an argument matcher in the current <see cref="CallContext{Queue{IArgumentMatcher}}"/> 
        /// and returns a default value for <typeparamref name="T"/>.
        /// </summary>
        public static T Push<T>(IArgumentMatcher matcher)
        {
            CallContext<Queue<IArgumentMatcher>>.GetData(() => new Queue<IArgumentMatcher>())
               .Enqueue(matcher);

            // Pushing new matchers clears the last known filter.
            lastFilter.Value = null;

            return default(T);
        }

        internal static void Freeze(IMethodInvocation invocation)
        {
            var currentMatchers = CallContext<Queue<IArgumentMatcher>>.GetData(() => new Queue<IArgumentMatcher>());
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

            lastFilter.Value = new InvocationFilter(invocation, finalMatchers);
        }
    }
}
