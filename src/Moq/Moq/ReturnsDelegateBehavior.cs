using System;
using System.Diagnostics;
using System.Linq;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    [DebuggerDisplay("{@delegate}", Name = "Returns", Type = nameof(ReturnsDelegateBehavior))]
    class ReturnsDelegateBehavior : IBehavior
    {
        [DebuggerDisplay("<function>")]
        Delegate @delegate;

        public ReturnsDelegateBehavior(Delegate @delegate) => this.@delegate = @delegate;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        InvokeBehavior IBehavior.Invoke => Invoke;

        IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            var arguments = invocation.Arguments.ToArray();
            var parameters = invocation.MethodBase.GetParameters();

            var returnValue = @delegate.DynamicInvoke(arguments);

            return invocation.CreateValueReturn(returnValue, arguments);
        }
    }
}
