﻿using System;
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
                var returnBehavior = behavior.Behaviors.OfType<ReturnsBehavior>().FirstOrDefault();
                if (returnBehavior != null)
                {
                    returnBehavior.ValueGetter = () => value;
                }
                else
                {
                    behavior.Behaviors.Add(new ReturnsBehavior(() => value));
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
                var returnBehavior = behavior.Behaviors.OfType<ReturnsBehavior>().FirstOrDefault();
                if (returnBehavior != null)
                {
                    returnBehavior.ValueGetter = () => value();
                }
                else
                {
                    behavior.Behaviors.Add(new ReturnsBehavior(() => value()));
                }
            }

            return default(TResult);
        }


        static TResult Returns<TResult>(Delegate value, InvokeBehavior behavior)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                // TODO: Is this even necessary given that intellisense gives us 
                // the rigth compiler safety already?
                setup.Invocation.EnsureCompatible(value);

                var mock = ((IMocked)setup.Invocation.Target).Mock;

                mock.Invocations.Remove(setup.Invocation);
                var mockBehavior = mock.BehaviorFor(setup);

                mockBehavior.Behaviors.Add(new Behavior(behavior, "Returns(() => ...)"));
            }

            return default(TResult);
        }
    }
}
