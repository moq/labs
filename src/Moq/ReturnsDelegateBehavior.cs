using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Avatars;
using Moq.Sdk;

namespace Moq
{
    [DebuggerDisplay("{@delegate}", Name = "Returns", Type = nameof(ReturnsDelegateBehavior))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    class ReturnsDelegateBehavior : IMockBehavior
    {
        [DebuggerDisplay("<function>")]
        readonly Delegate @delegate;

        public ReturnsDelegateBehavior(Delegate @delegate) => this.@delegate = @delegate;

        public IMethodReturn Execute(IMock mock, IMethodInvocation invocation, GetNextMockBehavior next)
        {
            var arguments = invocation.Arguments.Select(prm => invocation.Arguments.GetValue(prm.Name)).ToArray();
            var returnValue = @delegate.DynamicInvoke(arguments);

            return invocation.CreateValueReturn(returnValue, arguments);
        }
    }
}
