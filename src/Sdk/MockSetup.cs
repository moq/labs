using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Moq.Proxy;

namespace Moq.Sdk
{
    public class MockSetup
    {
        static AsyncLocal<IMockSetup> lastSetup = new AsyncLocal<IMockSetup>();

        public static IMockSetup Current => lastSetup.Value;

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

            // Pushing new matchers clears the last known setup.
            lastSetup.Value = null;

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

            lastSetup.Value = new MockSetupImpl(invocation, finalMatchers.ToArray());
        }
    }
}
