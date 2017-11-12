using System;
using System.Collections.Generic;
using System.Text;
using Moq.Proxy;
using Moq.Sdk;

namespace Moq
{
    public class ReturnValue
    {
        public ReturnValue(Func<object> valueGetter) => this.ValueGetter = valueGetter;

        public Func<object> ValueGetter { get; set; }
    }

    internal class ReturnsInvocationBehavior : InvocationBehavior
    {
        public ReturnsInvocationBehavior(InvokeBehavior invoke, ReturnValue value, string name = null) : base(invoke, name, "Returns")
        {
            ReturnValue = value;
        }

        public ReturnValue ReturnValue { get; set; }
    }
}
