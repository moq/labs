using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Moq.Proxy
{
    /// <summary>
    /// Encapsulates a list of <see cref="IProxyBehavior"/>s
    /// and manages calling them in the proper order with the right inputs.
    /// </summary>
    public class BehaviorPipeline
    {
        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/> with the given set of 
        /// <see cref="InvokeBehavior"/> delegates.
        /// </summary>
        /// <param name="behaviors">Behaviors to add to the pipeline.</param>
        public BehaviorPipeline(params InvokeBehavior[] behaviors)
            : this((IEnumerable<InvokeBehavior>)behaviors)
        {
        }

        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/> with the given set of 
        /// <see cref="InvokeBehavior"/> delegates.
        /// </summary>
        /// <param name="behaviors">Behaviors to add to the pipeline.</param>
        public BehaviorPipeline(IEnumerable<InvokeBehavior> behaviors)
            : this(behaviors.Select(behavior => ProxyBehavior.Create(behavior)))
        {
        }

        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/> with the given set of <see cref="IProxyBehavior"/>s.
        /// </summary>
        /// <param name="behaviors">Behaviors to add to the pipeline.</param>
        public BehaviorPipeline(params IProxyBehavior[] behaviors)
            : this((IEnumerable<IProxyBehavior>)behaviors)
        {
        }

        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/> with the given set of <see cref="IProxyBehavior"/>s.
        /// </summary>
        /// <param name="behaviors">Behaviors to add to the pipeline.</param>
        public BehaviorPipeline(IEnumerable<IProxyBehavior> behaviors)
        {
            Behaviors = new ObservableCollection<IProxyBehavior>(behaviors);
        }

        /// <summary>
        /// Creates a new <see cref="BehaviorPipeline"/>.
        /// </summary>
        public BehaviorPipeline()
        {
            Behaviors = new ObservableCollection<IProxyBehavior>();
        }

        public ObservableCollection<IProxyBehavior> Behaviors { get; }

        /// <summary>
        /// Invoke the pipeline with the given input.
        /// </summary>
        /// <param name="invocation">Input to the method call.</param>
        /// <param name="target">The ultimate target of the call.</param>
        /// <param name="throwOnException">Whether to throw the <see cref="IMethodReturn.Exception"/> if it has a value after running 
        /// the beaviors.</param>
        /// <returns>Return value from the pipeline.</returns>
        public IMethodReturn Invoke(IMethodInvocation invocation, InvokeBehavior target, bool throwOnException = false)
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

            var result = Behaviors[index].Invoke(invocation, () =>
            {
                for (index++; index < Behaviors.Count; index++)
                {
                    if (Behaviors[index].AppliesTo(invocation))
                        break;
                }

                return (index < Behaviors.Count) ?
                    Behaviors[index].Invoke :
                    target;
            });

            if (throwOnException && result.Exception != null)
                throw result.Exception;

            return result;
        }
    }
}
