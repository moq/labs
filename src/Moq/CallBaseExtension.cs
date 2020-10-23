using Moq.Sdk;
using System.ComponentModel;

namespace Moq
{
    /// <summary>
    /// Extensions for calling base member virtual implementation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CallBaseExtension
    {
        /// <summary>
        /// Specifies to call the base member virtual implementations by default.
        /// </summary>
        public static T CallBase<T>(this T target)
        {
            if (target is IMocked mocked && mocked != null)
            {
                // Configure CallBase at the Mock level
                mocked.AsMoq().CallBase = true;
            }
            else if (MockContext.CurrentInvocation != null)
            {
                // Configure CallBase at the invocation level
                MockContext.CurrentInvocation.Target.AsMock()
                    .GetPipeline(MockContext.CurrentSetup ?? CallContext.ThrowUnexpectedNull<IMockSetup>())
                    .Behaviors.Add(new DelegateMockBehavior(
                         (m, i, next) =>
                         {
                             // set CallBase
                             i.Context[nameof(IMoq.CallBase)] = true;
                             return next().Invoke(i.Target.AsMock(), i, next);
                         }, 
                         nameof(IMoq.CallBase)));
            }
            // TODO: else throw?

            return target;
        }
    }
}