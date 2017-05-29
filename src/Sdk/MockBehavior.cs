using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Creates a new behaviors for a mock.
    /// </summary>
    public class MockBehavior : IMockBehavior
    {
        /// <summary>
        /// Creates an <see cref="IMockBehavior"/> from an anonymous delegate/lambda.
        /// </summary>
        /// <param name="appliesTo">The condition for the <see cref="IMockBehavior.AppliesTo(IMethodInvocation)"/> method.</param>
        /// <param name="behavior">The behavior to invoke for <see cref="IProxyBehavior.Invoke(IMethodInvocation, GetNextBehavior)"/> 
        /// whenever the <paramref name="appliesTo"/> condition is satisfied by the current method invocation.</param>
        /// <param name="name">Optional name of the anonymous behavior to add.</param>
        public static IMockBehavior Create(InvokeBehavior behavior, string name = null) =>
            new MockBehavior(MockSetup.Current, behavior, name);

        InvokeBehavior behavior;
        string name;

        public MockBehavior(IMockSetup setup, InvokeBehavior behavior, string name)
        {
            Setup = setup;
            this.behavior = behavior;
            this.name = name;
        }

        public IMockSetup Setup { get; }

        /// <summary>
        /// Delegates to the <see cref="IMockSetup.AppliesTo(IMethodInvocation)"/> to determine 
        /// if the current behavior applies to the given invocation.
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => Setup.AppliesTo(invocation);

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) => behavior.Invoke(invocation, getNext);

        public override string ToString() => name ?? "custom";
    }
}
