using System.Collections.Generic;
using Moq.Proxy;
using Moq.Sdk;
using System.Reflection;

namespace Moq
{
    public static class MockExtensions
    {
        public static void Returns<T>(this object target, T value)
        {
            var invocation = CallContext<IMethodInvocation>.GetData(nameof(IMethodInvocation));

            var mock = (IMock)invocation.Target;

            var currentMatchers = CallContext<Stack<IArgumentMatcher>>.GetData(nameof(IArgumentMatcher), () => new Stack<IArgumentMatcher>());
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
                    finalMatchers.Add(currentMatchers.Pop());
                }
                else
                {
                    finalMatchers.Add(new ConstantArgumentMatcher(parameter.ParameterType, argument));
                }
            }

            mock.Invocations.Remove(invocation);

            mock.AddMockBehavior(new ArgumentMatcherFilter(invocation, finalMatchers).AppliesTo, (mi, next) => mi.CreateValueReturn(value, mi.Arguments));
        }
    }
}
