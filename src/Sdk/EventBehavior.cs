using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// A <see cref="IProxyBehavior"/> that keeps track of backing delegates 
    /// for events, combining and removing handlers from them as += and -= is 
    /// invoked on the mock.
    /// </summary>
    public class EventBehavior : IProxyBehavior
    {
        /// <inheritdoc />
        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            var isEventAddOrRemove = invocation.MethodBase.IsSpecialName &&
                (invocation.MethodBase.Name.StartsWith("add_") || invocation.MethodBase.Name.StartsWith("remove_"));

            if (!isEventAddOrRemove)
                return getNext()(invocation, getNext);

            var info = invocation.MethodBase.DeclaringType.GetRuntimeEvent(
                invocation.MethodBase.Name.Replace("add_", string.Empty).Replace("remove_", string.Empty));

            if (info != null)
            {
                EventRaiser raiser = null;
                if ((raiser = CallContext<EventRaiser>.GetData()) != null)
                {
                    try
                    {
                        var mock = ((IMocked)invocation.Target).Mock;
                        if (mock.State.TryGetValue<Delegate>(info.Name, out var handler) && 
                            handler != null)
                        {
                            switch (raiser)
                            {
                                case EmptyEventRaiser _:
                                    handler.DynamicInvoke(invocation.Target, EventArgs.Empty);
                                    break;
                                case EventArgsEventRaiser a:
                                    handler.DynamicInvoke(invocation.Target, a.EventArgs);
                                    break;
                                case CustomEventRaiser c:
                                    handler.DynamicInvoke(c.Arguments);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    finally 
                    {
                        CallContext<EventRaiser>.SetData(null);
                    }
                }
                else
                {
                    var handler = invocation.Arguments.FirstOrDefault() as Delegate;
                    if (handler != null)
                    {
                        var mock = ((IMocked)invocation.Target).Mock;
                        if (invocation.MethodBase.Name.StartsWith("add_"))
                            CombineDelegate(info, handler, mock);
                        else
                            RemoveDelegate(info, handler, mock);
                    }
                }
            }

            return getNext()(invocation, getNext);
        }

        static void CombineDelegate(EventInfo info, Delegate handler, IMock mock)
        {
            var state = mock.State.GetOrAdd(info.Name, () => handler);
            if (state != handler)
            {
                // Try without locking first
                var newState = Delegate.Combine(state, handler);
                if (!mock.State.TryUpdate(info.Name, newState, state))
                {
                    lock (mock.State)
                    {
                        // Retry with locking
                        state = mock.State.GetOrAdd(info.Name, () => handler);
                        if (state != handler)
                        {
                            // This should never fail, since we're inside the lock
                            Debug.Assert(
                                mock.State.TryUpdate(Delegate.Combine(state, handler), state));
                        }
                    }
                }
            }
        }

        static void RemoveDelegate(EventInfo info, Delegate handler, IMock mock)
        {
            if (mock.State.TryGetValue<Delegate>(info.Name, out var state))
            {
                // Try without locking first
                var newState = Delegate.Remove(state, handler);
                if (newState != state)
                {
                    if (!mock.State.TryUpdate(info.Name, newState, state))
                    {
                        lock (mock.State)
                        {
                            // Retry with locking
                            if (mock.State.TryGetValue(info.Name, out state))
                            {
                                newState = Delegate.Remove(state, handler);
                                if (newState != state)
                                {
                                    // This should never fail, since we're inside the lock
                                    Debug.Assert(
                                        mock.State.TryUpdate(newState, state));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
