using System.Collections.Generic;
using Moq.Proxy;
using Moq.Sdk;
using System.Reflection;
using System.ComponentModel;

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
        public static void Returns<T>(this object target, T value)
        {
            var invocation = CallContext<IMethodInvocation>.GetData(nameof(IMethodInvocation));

            var mock = ((IMocked)invocation.Target).Mock;
            
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

            mock.Invocations.Remove(invocation);
            mock.AddBehavior(new SimpleBehaviorFilter(invocation, finalMatchers).AppliesTo, (mi, next) => mi.CreateValueReturn(value, mi.Arguments));
        }
    }
}
