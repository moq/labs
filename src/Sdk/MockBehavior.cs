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
        /// <param name="name">Optional name of the anonymous behavior to add.</param>
        public static IMockBehavior Create(Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior, string name = null) =>
            new AnonymousMockBehavior(appliesTo, behavior, name);

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            // Allows subsequent extension methods on the fluent API to retrieve the 
            // current invocation being performed.
            CallContext<IMethodInvocation>.SetData(invocation);

            // Determines the current invocation filter according to contextual 
            // matchers that may have been pushed to the Matchers context. 
            Matchers.Freeze(invocation);

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
            string name;

            public AnonymousMockBehavior(Func<IMethodInvocation, bool> appliesTo, InvokeBehavior behavior, string name)
            {
                this.appliesTo = appliesTo;
                this.behavior = behavior;
                this.name = name;
            }

            public bool AppliesTo(IMethodInvocation invocation) => appliesTo(invocation);

            public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) => behavior(invocation, getNext);

            public override string ToString() => name ?? "custom";
        }
    }
}
