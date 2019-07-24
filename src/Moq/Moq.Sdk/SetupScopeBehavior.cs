using System.Linq;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// A behavior that skips all behaviors that do not apply during a setup scope.
    /// </summary>
    public class SetupScopeBehavior : IStuntBehavior
    {
        /// <summary>
        /// Applies only if <see cref="SetupScope.IsActive"/> is <see langword="true"/>.
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => SetupScope.IsActive;

        /// <summary>
        /// Skips all non-setup behaviors from execution.
        /// </summary>
        public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next)
        {
            foreach (var behavior in invocation.Target.AsMock().Behaviors.Where(x => !(x is DefaultValueBehavior) && !(x is MockContextBehavior)))
            {
                invocation.SkipBehaviors.Add(behavior.GetType());
            }

            return next().Invoke(invocation, next);
        }
    }
}
