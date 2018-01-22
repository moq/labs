using System;
using System.ComponentModel;
using System.Linq;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Provides mock verification methods.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class VerifyExtension
    {
        /// <summary>
        /// Verifies the mock received at least one invocation that matches 
        /// the given lambda.
        /// </summary>
        public static void Verify<T>(this T mock, Action<T> action)
        {
            using (new SetupScope())
            {
                action(mock);
                var setup = MockContext.CurrentSetup;
                var info = mock.GetMock();
                if (!info.Invocations.Any(i => setup.AppliesTo(i)))
                    throw new VerifyException<T>(info, setup);
            }
        }

        /// <summary>
        /// Verifies the mock received at least one invocation that matches 
        /// the given lambda.
        /// </summary>
        public static void Verify<T, TResult>(this T mock, Func<T, TResult> function)
        {
            using (new SetupScope())
            {
                function(mock);
                var setup = MockContext.CurrentSetup;
                var info = mock.GetMock();
                if (!info.Invocations.Any(i => setup.AppliesTo(i)))
                    throw new VerifyException<T>(info, setup);
            }
        }
    }
}
