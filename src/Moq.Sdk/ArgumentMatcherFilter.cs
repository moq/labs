using System;
using System.Collections.Generic;
using System.Text;
using Moq.Proxy;

namespace Moq.Sdk
{
    public class ArgumentMatcherFilter
    {
        IMethodInvocation setup;
        IList<IArgumentMatcher> matchers;

        public ArgumentMatcherFilter(IMethodInvocation setup, IList<IArgumentMatcher> matchers)
        {
            this.setup = setup;
            this.matchers = matchers;
        }

        public bool AppliesTo(IMethodInvocation invocation)
        {
            if (setup.MethodBase != invocation.MethodBase)
                return false;

            if (invocation.Arguments.Count != matchers.Count)
                return false;

            for (var i = 0; i < invocation.Arguments.Count; i++)
            {
                if (!matchers[i].Matches(invocation.Arguments[i]))
                    return false;
            }

            return true;
        }
    }
}