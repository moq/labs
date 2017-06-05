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
