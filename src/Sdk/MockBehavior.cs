using System.Linq;
using Moq.Proxy;
using System;
using Moq.Sdk.Properties;

namespace Moq.Sdk
{
    /// <summary>
    /// An <see cref="IProxyBehavior"/> that allows configuring <see cref="IMockBehavior"/>s
    /// that can dynamically determine whether they should be applied to the 
    /// current <see cref="IMethodInvocation"/>.
    /// </summary>
    public class MockBehavior : IProxyBehavior
    {
        /// <summary>
        /// Creates an <see cref="IMockBehavior"/> from an anonymous delegate/lambda.
        /// </summary>
        /// <param name="appliesTo">The condition for the <see cref="IMockBehavior.AppliesTo(IMethodInvocation)"/> method.</param>
        /// <param name="behavior">The behavior to invoke for <see cref="IProxyBehavior.Invoke(IMethodInvocation, GetNextBehavior)"/> 
        /// whenever the <paramref name="appliesTo"/> condition is satisfied by the current method invocation.</param>
        public static IMockBehavior Create(Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior) =>
            new AnonymousMockBehavior(appliesTo, behavior);

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            CallContext<IMethodInvocation>.SetData(invocation);

            var mock = invocation.Target is IMocked mocked ?
                mocked.Mock : throw new ArgumentException(Resources.TargetNotMocked);

            mock.Invocations.Add(invocation);

            if (mock.Behaviors.Count == 0)
                return getNext().Invoke(invocation, getNext);

            // This is the only added functionality of this behavior, to first match 
            // applicable InvokeBehaviors and execute them in sequence.
            var applicableBehaviors = mock.Behaviors.Where(behavior => behavior.AppliesTo(invocation)).ToArray();
            if (applicableBehaviors.Length == 0)
                return getNext().Invoke(invocation, getNext);

            var index = 0;
            return applicableBehaviors[0].Invoke(invocation, () =>
            {
                ++index;
                return (index < applicableBehaviors.Length) ?
                    applicableBehaviors[index].Invoke :
                    getNext();
            });
        }

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
