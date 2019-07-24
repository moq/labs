using System.ComponentModel;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Configures the current invocation depending on the <see cref="MockBehavior"/> 
    /// specified for the mock.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ConfigurePipelineBehavior : IStuntBehavior
    {
        /// <summary>
        /// Always applies to all invocations.
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => true;

        /// <summary>
        /// Configures the current invocation depending on the <see cref="MockBehavior"/> 
        /// specified for the mock.
        /// </summary>
        public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next)
        {
            var moq = invocation.Target.AsMoq();
            if (moq.Behavior != MockBehavior.Strict)
            {
                invocation.SkipBehaviors.Add(typeof(StrictMockBehavior));
            }

            return next().Invoke(invocation, next);
        }
    }
}
