using System;
using System.Collections.ObjectModel;
using System.Linq;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IMockBehavior"/>, which implements 
    /// effectively a sub-pipeline of behaviors within the <see cref="IStunt"/>'s 
    /// <see cref="BehaviorPipeline"/>, using the <see cref="IMockSetup.AppliesTo(IMethodInvocation)"/> 
    /// abstraction over the <see cref="IStuntBehavior.AppliesTo(IMethodInvocation)"/> member 
    /// to determine which ones to apply.
    /// </summary>
    /// <devdoc>
    /// 
    /// </devdoc>
    public class DefaultMockBehavior : IMockBehavior
    {
        public DefaultMockBehavior(IMockSetup setup) => Setup = setup;

        public ObservableCollection<IBehavior> Behaviors { get; } = new ObservableCollection<IBehavior>();

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
