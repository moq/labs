using Stunts;
using Moq.Sdk;
using System.ComponentModel;
using System;

namespace Moq
{
    /// <summary>
    /// Extensions for throwing exception from mock invocations.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static partial class ThrowsExtension
    {
        /// <summary>
        /// Specifies the exception to throw when the method is invoked.
        /// </summary>
        public static void Throws(this object target, Exception exception)
        {
            var setup = MockSetup.Current;
            if (setup != null)
            {
                var mock = ((IMocked)setup.Invocation.Target).Mock;

                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.BehaviorFor(setup);

                behavior.Behaviors.Add(new InvocationBehavior(
                    (mi, next) => mi.CreateExceptionReturn(exception),
                    "Exception", "Exception")
               );
            }
        }

        /// <summary>
        /// Specifies the exception to throw when the method is invoked.
        /// </summary>
        public static void Throws<TException>(this object target)
            where TException: Exception, new()
        {
            var setup = MockSetup.Current;
            if (setup != null)
            {
                var mock = ((IMocked)setup.Invocation.Target).Mock;

                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.BehaviorFor(setup);

                behavior.Behaviors.Add(new InvocationBehavior(
                    (mi, next) => mi.CreateExceptionReturn(new TException()),
                    "Exception", "Exception")
               );
            }
        }
    }
}
