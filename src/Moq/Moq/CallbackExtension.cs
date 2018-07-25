using Stunts;
using Moq.Sdk;
using System.ComponentModel;
using System;

namespace Moq
{
    /// <summary>
    /// Extensions for configuring callbacks when invoking mocks.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static partial class CallbackExtension
    {
        static TResult Callback<TResult>(this TResult target, Action<IArgumentCollection> callback)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                var mock = setup.Invocation.Target.GetMock();

                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.GetPipeline(setup);

                // If there is already a behavior wrap it instead, 
                // so we can do a callback after even if it's a 
                // short-circuiting one like Returns.
                if (behavior.Behaviors.Count > 0)
                {
                    var wrapped = behavior.Behaviors.Pop();
                    behavior.Behaviors.Add(Sdk.MockBehavior.Create(
                        (mi, next) =>
                        {
                            // If the wrapped target does not invoke the next 
                            // behavior (us), then we invoke the callback explicitly.
                            var called = false;

                            // Note we're tweaking the GetNextBehavior to always 
                            // call us, before invoking the actual next behavior.
                            var result = wrapped.Execute(mi, () => (IMethodInvocation _, GetNextBehavior __) =>
                            {
                                callback(mi.Arguments);
                                called = true;
                                return next()(mi, next);
                            });

                            // The Returns behavior does not invoke the GetNextBehavior, 
                            // and therefore we won't have been called in that case, 
                            // so call the callback before returning.
                            if (!called)
                                callback(mi.Arguments);

                            return result;
                        }, "Callback")
                   );
                }
                else
                {
                    behavior.Behaviors.Add(Sdk.MockBehavior.Create(
                        (mi, next) =>
                        {
                            callback(mi.Arguments);
                            return next()(mi, next);
                        }, "Callback")
                   );
                }
            }

            return target;
        }
    }
}
