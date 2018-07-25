using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IMockBehaviorPipeline"/>, which 
    /// provides a sub-pipeline of behaviors within the <see cref="IStunt"/>'s 
    /// <see cref="BehaviorPipeline"/>, which is only run if the current invocation 
    /// matches the <see cref="Setup"/>.
    /// </summary>
    [DebuggerDisplay("{Setup}")]
    public class MockBehaviorPipeline : IMockBehaviorPipeline
    {
        public MockBehaviorPipeline(IMockSetup setup) => Setup = setup;

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public ObservableCollection<IMockBehavior> Behaviors { get; } = new ObservableCollection<IMockBehavior>();

        public IMockSetup Setup { get; }

        /// <summary>
        /// Delegates to the <see cref="IMockSetup.AppliesTo(IMethodInvocation)"/> to determine 
        /// if the current behavior applies to the given invocation.
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => Setup.AppliesTo(invocation);

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior next)
        {
            // NOTE: the mock behavior is like a sub-pipeline within the overall stunt 
            // behavior pipeline, where all the behaviors added automatically apply as 
            // since they all share the same AppliesTo implementation, which is the setup 
            // itself.

            if (Behaviors.Count == 0)
                return next().Invoke(invocation, next);

            var index = 0;
            var result = Behaviors[0].Execute(invocation, () =>
            {
                ++index;
                return (index < Behaviors.Count) ?
                    Behaviors[index].Execute :
                    next();
            });

            return result;
        }
    }
}
