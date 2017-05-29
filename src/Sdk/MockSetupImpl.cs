using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq.Proxy;

namespace Moq.Sdk
{
    internal class MockSetupImpl : IMockSetup
    {
        IArgumentMatcher[] matchers;
        Lazy<string> toDisplay;

        public MockSetupImpl(IMethodInvocation invocation, IArgumentMatcher[] matchers)
        {
            Invocation = invocation;
            this.matchers = matchers;
            toDisplay = new Lazy<string>(ToDisplay);
        }

        public IMethodInvocation Invocation { get; }

        public IArgumentMatcher[] Matchers => matchers;

        public bool AppliesTo(IMethodInvocation actualInvocation)
        {
            if (actualInvocation == null)
                throw new ArgumentNullException(nameof(actualInvocation));

            if (Invocation.MethodBase != actualInvocation.MethodBase)
                return false;

            if (actualInvocation.Arguments.Count != matchers.Length)
                return false;

            for (var i = 0; i < actualInvocation.Arguments.Count; i++)
            {
                if (!matchers[i].Matches(actualInvocation.Arguments[i]))
                    return false;
            }

            return true;
        }

        public override string ToString() => toDisplay.Value;

        string ToDisplay()
        {
            var result = new StringBuilder();
            if (Invocation.MethodBase is MethodInfo info)
            {
                if (info.ReturnType != typeof(void))
                    result.Append(Stringly.ToTypeName(info.ReturnType)).Append(" ");
                else
                    result.Append("void ");
            }

            result.Append(Invocation.MethodBase.Name);
            if (Invocation.MethodBase.IsGenericMethod)
            {
                var generic = ((MethodInfo)Invocation.MethodBase).GetGenericMethodDefinition();
                result
                    .Append("<")
                    .Append(string.Join(", ", generic.GetGenericArguments().Select(t => t.Name)))
                    .Append(">");
            }

            var parameters = Invocation.MethodBase.GetParameters();

            return result
                .Append("(")
                .Append(string.Join(", ", parameters.Select((p, i) =>
                    Stringly.ToTypeName(p.ParameterType) + " " +
                    p.Name + " = " +
                    matchers[i].ToString()
                )))
                .Append(")")
                .ToString();
        }
    }
}
