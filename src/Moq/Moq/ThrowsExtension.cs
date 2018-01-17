using Moq.Sdk;
using System.ComponentModel;
using System;

namespace Moq
{
    /// <summary>
    /// Extensions for throwing exception from mock invocations.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class ThrowsExtension
    {
        /// <summary>
        /// Specifies the exception to throw when the method is invoked.
        /// </summary>
        public static void Throws(this object target, Exception exception)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                var mock = ((IMocked)setup.Invocation.Target).Mock;

                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.BehaviorFor(setup);

                behavior.Behaviors.Add(new Behavior(
                    (mi, next) => mi.CreateExceptionReturn(exception),
                    new Lazy<string>(() => $"Throws<{exception.GetType().Name}>(\"{exception.Message}\")")
               ));
            }
        }

        /// <summary>
        /// Specifies the exception to throw when the method is invoked.
        /// </summary>
        public static void Throws<TException>(this object target)
            where TException: Exception, new()
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                var mock = ((IMocked)setup.Invocation.Target).Mock;

                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.BehaviorFor(setup);

                behavior.Behaviors.Add(new Behavior(
                    (mi, next) => mi.CreateExceptionReturn(new TException()),
                    new Lazy<string>(() => $"Throws<{typeof(TException).Name}>()")
               ));
            }
        }
    }
}
