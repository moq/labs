using Moq.Sdk;
using Stunts;

namespace Moq
{
    internal class ConfigurePipelineBehavior : IStuntBehavior
    {
        public bool AppliesTo(IMethodInvocation invocation) => true;

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
