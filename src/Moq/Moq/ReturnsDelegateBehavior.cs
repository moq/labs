using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    [DebuggerDisplay("{@delegate}", Name = "Returns", Type = nameof(ReturnsDelegateBehavior))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ReturnsDelegateBehavior : IMockBehavior
    {
        [DebuggerDisplay("<function>")]
        Delegate @delegate;

        public ReturnsDelegateBehavior(Delegate @delegate) => this.@delegate = @delegate;

        public IMethodReturn Execute(IMock mock, IMethodInvocation invocation, GetNextMockBehavior next)
        {
            var arguments = invocation.Arguments.ToArray();
            var parameters = invocation.MethodBase.GetParameters();

            var returnValue = @delegate.DynamicInvoke(arguments);

            return invocation.CreateValueReturn(returnValue, arguments);
        }
    }
}
