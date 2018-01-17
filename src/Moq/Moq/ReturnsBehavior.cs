using System;
using Stunts;
using Moq.Sdk;
using System.Diagnostics;

namespace Moq
{
    /// <summary>
    /// A custom invocation info for returning values, so that 
    /// the actual value to return can be replaced on succesive 
    /// <see cref="ReturnsExtension"/> method calls.
    /// </summary>
    [DebuggerDisplay("Returns {ReturnValue}")]
    class ReturnsBehavior : IBehavior
    {
        public ReturnsBehavior(Func<object> valueGetter)
            => ValueGetter = valueGetter;

        public Func<object> ValueGetter { get; set; }

        public InvokeBehavior Invoke => (IMethodInvocation invocation, GetNextBehavior getNext)
            => invocation.CreateValueReturn(ValueGetter());
    }
}
