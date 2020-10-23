using System;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Allows raising events from mocks.
    /// </summary>
    public static class Raise
    {
        /// <summary>
        /// Raises the event being attached to, passing the target mock 
        /// as the sender, and <see cref="EventArgs.Empty"/> args.
        /// </summary>
        public static EventHandler? Event()
        {
            CallContext<EventRaiser>.SetData(EventRaiser.Empty);
            return default;
        }

        /// <summary>
        /// Raises the event being attached to, passing the target mock 
        /// as the sender, and the given <paramref name="args"/> as the 
        /// event arguments.
        /// </summary>
        public static EventHandler<TEventArgs>? Event<TEventArgs>(TEventArgs args)
        {
            CallContext<EventRaiser>.SetData(new EventArgsEventRaiser(args));
            return default;
        }

        /// <summary>
        /// Raises the event being attached to, passing the target mock 
        /// as the sender, and the given <paramref name="args"/> as the 
        /// event arguments.
        /// </summary>
        public static EventHandler? Event(EventArgs args)
        {
            CallContext<EventRaiser>.SetData(new EventArgsEventRaiser(args));
            return default;
        }

        /// <summary>
        /// Raises the event being attached to, passing the target mock 
        /// as the sender, and the given <paramref name="args"/> as the 
        /// event arguments.
        /// </summary>
        public static TEventHandler? Event<TEventHandler>(EventArgs args)
        {
            CallContext<EventRaiser>.SetData(new EventArgsEventRaiser(args));
            return default;
        }

        /// <summary>
        /// Raises the event being attached to, passing the given 
        /// <paramref name="args"/> as the event arguments.
        /// </summary>
        public static TEventHandler? Event<TEventHandler>(params object[] args)
        {
            CallContext<EventRaiser>.SetData(new CustomEventRaiser(args));
            return default;
        }
    }
}
