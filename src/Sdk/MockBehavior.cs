using System;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// A factory for a <see cref="IMockBehavior"/> from lambdas.
    /// </summary>
    public class MockBehavior
    {
        public static IMockBehavior Create(Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior) =>
            new AnonymousMockBehavior(appliesTo, behavior);

        class AnonymousMockBehavior : IMockBehavior
        {
            Func<IMethodInvocation, bool> appliesTo;
            InvokeBehavior behavior;

            public AnonymousMockBehavior(Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior)
            {
                this.appliesTo = appliesTo;
                this.behavior = behavior;
            }

            public bool AppliesTo(IMethodInvocation invocation) => appliesTo(invocation);

            public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) => behavior(invocation, getNext);
        }
    }
}
