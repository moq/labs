using System;
using Stunts;
using Moq.Sdk;
using System.Diagnostics;

namespace Moq
{
    /// <summary>
    /// A custom behavior for returning values, so that 
    /// the actual value to return can be replaced on succesive 
    /// <see cref="ReturnsExtension"/> method calls.
    /// </summary>
    [DebuggerDisplay("{DebuggerValue}", Name = "Returns", Type = nameof(ReturnsBehavior))]
    class ReturnsBehavior : IBehavior
    {
        public ReturnsBehavior(Func<object> valueGetter)
            => ValueGetter = valueGetter;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Func<object> ValueGetter { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public InvokeBehavior Invoke => (IMethodInvocation invocation, GetNextBehavior getNext)
            => invocation.CreateValueReturn(ValueGetter());

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object DebuggerValue => ValueGetter();
    }
}
