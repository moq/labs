using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Evaluates a list of <see cref="IArgumentMatcher"/>s against an 
    /// <see cref="IMethodInvocation"/> to determine if it satisfies the 
    /// requirements against the <see cref="IMethodInvocation"/> used 
    /// when the behavior was originally set up.
    /// </summary>
    public class InvocationFilter
    {
        IMethodInvocation setup;
        IList<IArgumentMatcher> matchers;

        /// <summary>
        /// Creates an instance of the filter.
        /// </summary>
        /// <param name="setup">The invocation used to set up the behavior.</param>
        /// <param name="matchers">The matchers determined during the set up.</param>
        public InvocationFilter(IMethodInvocation setup, IList<IArgumentMatcher> matchers)
        {
            this.setup = setup;
            this.matchers = matchers;
        }

        /// <summary>
        /// Evaluates the filter against an actual invocation.
        /// </summary>
        /// <param name="invocation">The actual invocation being performed on the mock.</param>
        /// <returns>Whether the filter applies to the given invocation.</returns>
        public bool AppliesTo(IMethodInvocation actualInvocation)
        {
            if (actualInvocation == null)
                throw new System.ArgumentNullException(nameof(actualInvocation));

            if (setup.MethodBase != actualInvocation.MethodBase)
                return false;

            if (actualInvocation.Arguments.Count != matchers.Count)
                return false;

            for (var i = 0; i < actualInvocation.Arguments.Count; i++)
            {
                if (!matchers[i].Matches(actualInvocation.Arguments[i]))
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            if (setup.MethodBase is MethodInfo info)
            {
                if (info.ReturnType != typeof(void))
                    result.Append(Stringly.ToTypeName(info.ReturnType)).Append(" ");
                else
                    result.Append("void ");
            }

            result.Append(setup.MethodBase.Name);
            if (setup.MethodBase.IsGenericMethod)
            {
                var generic = ((MethodInfo)setup.MethodBase).GetGenericMethodDefinition();
                result
                    .Append("<")
                    .Append(string.Join(", ", generic.GetGenericArguments().Select(t => t.Name)))
                    .Append(">");
            }

            var parameters = setup.MethodBase.GetParameters();

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