namespace Moq.Sdk
{
    /// <summary>
    /// Base class for context state that signals an event must be 
    /// raised from <see cref="EventBehavior"/>.
    /// </summary>
    public abstract class EventRaiser
    {
        /// <summary>
        /// The event to raise is a simple <see cref="System.EventHandler"/>, 
        /// which will be raised with an <see cref="System.EventArgs.Empty"/> 
        /// value.
        /// </summary>
        public static EventRaiser Empty { get; } = new EmptyEventRaiser();
    }

    class EmptyEventRaiser : EventRaiser { }

    /// <summary>
    /// Context state that signals an event must be 
    /// raised from <see cref="EventBehavior"/> with the given 
    /// <see cref="EventArgs"/> value and the invocation target 
    /// as the sender.
    /// </summary>
    public class EventArgsEventRaiser : EventRaiser
    {
        /// <summary>
        /// Creates an instance of the <see cref="EventArgsEventRaiser"/> 
        /// with the given event arguments.
        /// </summary>
        public EventArgsEventRaiser(object? args) => EventArgs = args;

        /// <summary>
        /// The event arguments for the raised event.
        /// </summary>
        public object? EventArgs { get; }
    }

    /// <summary>
    /// Context state that signals a custom event must be 
    /// raised from <see cref="EventBehavior"/> with the given 
    /// <see cref="Arguments"/> passed in directly to the custom 
    /// delegate.
    /// </summary>
    public class CustomEventRaiser : EventRaiser
    {
        /// <summary>
        /// Creates an instance of the <see cref="EventArgsEventRaiser"/> 
        /// with the given event arguments.
        /// </summary>
        public CustomEventRaiser(object[] arguments) => Arguments = arguments;

        /// <summary>
        /// The event arguments for the raised custom event.
        /// </summary>
        public object[] Arguments { get; }
    }
}