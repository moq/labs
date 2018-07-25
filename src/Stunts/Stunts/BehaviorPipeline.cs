using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Stunts
{
    /// <summary>
    /// Encapsulates a list of <see cref="IStuntBehavior"/>s
    /// and manages calling them in the proper order with the right inputs.
    /// </summary>
    public class BehaviorPipeline
    {
        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/> with the given set of 
        /// <see cref="ExecuteDelegate"/> delegates.
        /// </summary>
        /// <param name="behaviors">Behaviors to add to the pipeline.</param>
        public BehaviorPipeline(params ExecuteDelegate[] behaviors)
            : this((IEnumerable<ExecuteDelegate>)behaviors)
        {
        }

        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/> with the given set of 
        /// <see cref="ExecuteDelegate"/> delegates.
        /// </summary>
        /// <param name="behaviors">Behaviors to add to the pipeline.</param>
        public BehaviorPipeline(IEnumerable<ExecuteDelegate> behaviors)
            : this(behaviors.Select(behavior => StuntBehavior.Create(behavior)))
        {
        }

        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/> with the given set of <see cref="IStuntBehavior"/>s.
        /// </summary>
        /// <param name="behaviors">Behaviors to add to the pipeline.</param>
        public BehaviorPipeline(params IStuntBehavior[] behaviors)
            : this((IEnumerable<IStuntBehavior>)behaviors)
        {
        }

        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/> with the given set of <see cref="IStuntBehavior"/>s.
        /// </summary>
        /// <param name="behaviors">Behaviors to add to the pipeline.</param>
        public BehaviorPipeline(IEnumerable<IStuntBehavior> behaviors)
        {
            Behaviors = new ObservableCollection<IStuntBehavior>(behaviors);
        }

        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/>.
        /// </summary>
        public BehaviorPipeline()
        {
            Behaviors = new ObservableCollection<IStuntBehavior>();
        }

        /// <summary>
        /// Gets the collection of behaviors applied to this instance.
        /// </summary>
        public ObservableCollection<IStuntBehavior> Behaviors { get; }

        /// <summary>
        /// Invoke the pipeline with the given input.
        /// </summary>
        /// <param name="invocation">Input to the method call.</param>
        /// <param name="target">The ultimate target of the call.</param>
        /// <param name="throwOnException">Whether to throw the <see cref="IMethodReturn.Exception"/> if it has a value after running 
        /// the behaviors.</param>
        /// <returns>Return value from the pipeline.</returns>
        public IMethodReturn Invoke(IMethodInvocation invocation, ExecuteDelegate target, bool throwOnException = false)
        {
            if (Behaviors.Count == 0)
                return target(invocation, null);

            var index = -1;
            for (var i = 0; i < Behaviors.Count; i++)
            {
                if (Behaviors[i].AppliesTo(invocation))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                return target(invocation, null);

            var result = Behaviors[index].Execute(invocation, () =>
            {
                for (index++; index < Behaviors.Count; index++)
                {
                    if (Behaviors[index].AppliesTo(invocation))
                        break;
                }

                return (index < Behaviors.Count) ?
                    Behaviors[index].Execute :
                    target;
            });

            if (throwOnException && result.Exception != null)
                throw result.Exception;

            return result;
        }
    }
}
