using System;
using System.ComponentModel;
using System.Linq;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Extensions for configuring return values from mock invocations.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static partial class ReturnsExtension
    {
        /// <summary>
        /// Sets the return value for a property or non-void method.
        /// </summary>
        public static TResult Returns<TResult>(this TResult target, TResult value)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                var mock = ((IMocked)setup.Invocation.Target).Mock;
                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.BehaviorFor(setup);
                var returnBehavior = behavior.Behaviors.OfType<ReturnsInvocationBehavior>().FirstOrDefault();
                if (returnBehavior != null)
                {
                    returnBehavior.ReturnValue.ValueGetter = () => value;
                }
                else
                {
                    var returnValue = new ReturnValue(() => value);
                    behavior.Behaviors.Add(new ReturnsInvocationBehavior(
                        (mi, next) => mi.CreateValueReturn(returnValue.ValueGetter(), mi.Arguments),
                        returnValue)
                    );
                }
            }

            return default(TResult);
        }

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<TResult>(this TResult target, Func<TResult> value)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                var mock = ((IMocked)setup.Invocation.Target).Mock;
                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.BehaviorFor(setup);
                var returnBehavior = behavior.Behaviors.OfType<ReturnsInvocationBehavior>().FirstOrDefault();
                if (returnBehavior != null)
                {
                    returnBehavior.ReturnValue.ValueGetter = () => value();
                }
                else
                {
                    var returnValue = new ReturnValue(() => value());
                    behavior.Behaviors.Add(new ReturnsInvocationBehavior(
                        (mi, next) => mi.CreateValueReturn(returnValue.ValueGetter(), mi.Arguments),
                        returnValue,
                        "Returns")
                    );
                }
            }

            return default(TResult);
        }


        static TResult Returns<TResult>(Delegate value, InvokeBehavior behavior)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                setup.Invocation.EnsureCompatible(value);
                var mock = ((IMocked)setup.Invocation.Target).Mock;

                mock.Invocations.Remove(setup.Invocation);
                var mockBehavior = mock.BehaviorFor(setup);

                mockBehavior.Behaviors.Add(new InvocationBehavior(behavior, "Returns", "Returns"));
            }

            return default(TResult);
        }
    }
}
