using System;
using Stunts;
using Moq.Sdk;

namespace Moq
{
    internal class ReturnsInvocationBehavior : InvocationBehavior
    {
        public ReturnsInvocationBehavior(InvokeBehavior invoke, ReturnValue value, string name = null) : base(invoke, name, "Returns")
            => ReturnValue = value;

        public ReturnValue ReturnValue { get; set; }
    }

    internal class ReturnValue
    {
        public ReturnValue(Func<object> valueGetter) => ValueGetter = valueGetter;

        public Func<object> ValueGetter { get; set; }
    }
}
