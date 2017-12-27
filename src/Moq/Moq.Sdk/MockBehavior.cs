using System;
using System.Collections.ObjectModel;
using System.Linq;
using Stunts;

namespace Moq.Sdk
{
    /// <inheritdoc />
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
            // NOTE: the mock behavior is like a sub-pipeline within the overall stunt 
            // behavior pipeline, where all the behaviors added automatically apply as 
            // since they all share the same AppliesTo implementation, which is the setup 
            // itself.

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
            => Setup + string.Join(Environment.NewLine, Behaviors.Select(x => "\t" + x));
    }
}
