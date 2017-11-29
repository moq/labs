using Moq.Proxy;

namespace Moq.Sdk
{
    public class InvocationBehavior
    {
        public InvocationBehavior(InvokeBehavior invoke, string name = null, string kind = null)
        {
            Invoke = invoke;
            Name = name;
            Kind = kind;
        }

        public InvokeBehavior Invoke { get; }

        public string Name { get; }

        public string Kind { get; }

        public override string ToString() => Name ?? "<unnamed>";
    }
}