using Moq.Proxy;

namespace Moq.Sdk
{
    public class InvocationBehavior
    {
        public InvocationBehavior(InvokeBehavior invoke, string name = null)
        {
            Invoke = invoke;
            Name = name;
        }

        public InvokeBehavior Invoke { get; }

        public string Name { get; }

        public override string ToString() => Name ?? "<unnamed>";
    }
}
