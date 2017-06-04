using System;
using System.Collections.ObjectModel;
using System.Linq;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Represents a mock's behavior.
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
        public static IMockBehavior Create(InvokeBehavior behavior, string name = null)
        {
            var setup = MockSetup.Current;
            var invocation = CallContext<IMethodInvocation>.GetData();
            if (setup != null && invocation is IMocked mocked)
            {
                return mocked.Mock.BehaviorFor(setup);
            }
            else
            {
                // TODO: improve message/recovery?
                throw new InvalidOperationException();
            }
        }

        public MockBehavior(IMockSetup setup) => Setup = setup;

        public ObservableCollection<InvocationBehavior> Behaviors { get; } = new ObservableCollection<InvocationBehavior>();

        public IMockSetup Setup { get; }

        /// <summary>
        /// Delegates to the <see cref="IMockSetup.AppliesTo(IMethodInvocation)"/> to determine 
        /// if the current behavior applies to the given invocation.
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => Setup.AppliesTo(invocation);

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            if (Behaviors.Count == 0)
                return getNext().Invoke(invocation, getNext);

            var index = 0;
            var result = Behaviors[0].Invoke(invocation, () =>
            {
                ++index;
                return (index < Behaviors.Count) ?
                    Behaviors[index].Invoke :
                    getNext();
            });

            return result;
        }

        // TODO: render all behaviors too?
        public override string ToString()
            => Setup.ToString() + string.Join(Environment.NewLine, Behaviors.Select(x => "\t" + x.ToString()));
    }
}
