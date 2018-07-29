using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Stunts;

namespace Moq.Sdk
{
    public partial class MockSetup
    {
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

            return default;
        }

        internal static IMockSetup Freeze(IMethodInvocation invocation)
        {
            var currentMatchers = CallContext<Queue<IArgumentMatcher>>.GetData(() => new Queue<IArgumentMatcher>());
            var finalMatchers = new List<IArgumentMatcher>();
            var parameters = invocation.MethodBase.GetParameters();
            var defaultValue = (invocation.Target as IMocked)?.
                Mock.Behaviors.OfType<DefaultValueBehavior>().FirstOrDefault()?.Provider ??
                new DefaultValue();

            for (var i = 0; i < invocation.Arguments.Count; i++)
            {
                var argument = invocation.Arguments[i];
                var parameter = parameters[i];

                // This is a bit fuzzy since we compare the actual argument value against the 
                // default value for the parameter type, or the type of the matcher in the 
                // queue of argument matchers to see if applies instead.
                if (object.Equals(argument, defaultValue.For(parameter.ParameterType)) &&
                    currentMatchers.Count != 0 &&
                    parameter.ParameterType.GetValueTypeInfo().IsAssignableFrom(currentMatchers.Peek().ArgumentType.GetValueTypeInfo()))
                {
                    finalMatchers.Add(currentMatchers.Dequeue());
                }
                else
                {
                    finalMatchers.Add(new ValueMatcher(parameter.ParameterType, argument));
                }
            }

            return new MockSetup(invocation, finalMatchers.ToArray());
        }
    }
}
