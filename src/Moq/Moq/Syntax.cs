using System;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Static class intended to be imported statically, like 
    /// <c>using static Moq.Syntax;</c>. Groups functionality 
    /// <see cref="Arg"/> and <see cref="Moq.Raise"/> 
    /// so it makes more sense in a statically imported context.
    /// </summary>
    public static class Syntax
    {
        /// <summary>
        /// Matches any value of the given type.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        public static T Any<T>() => Arg.Any<T>();

        /// <summary>
        /// Matches a value of the given type if it satisfies the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="condition">The condition to check against actual invocation values.</param>
        public static T Any<T>(Func<T, bool> condition) => Arg.Any(condition);

        /// <summary>
        /// Raises the event being attached to, passing the target mock 
        /// as the sender, and <see cref="EventArgs.Empty"/> args.
        /// </summary>
        public static EventHandler Raise() => Moq.Raise.Event();

        /// <summary>
        /// Raises the event being attached to, passing the target mock 
        /// as the sender, and the given <paramref name="args"/> as the 
        /// event arguments.
        /// </summary>
        public static EventHandler<TEventArgs> Raise<TEventArgs>(TEventArgs args) => Moq.Raise.Event<TEventArgs>(args);

        /// <summary>
        /// Raises the event being attached to, passing the target mock 
        /// as the sender, and the given <paramref name="args"/> as the 
        /// event arguments.
        /// </summary>
        public static EventHandler Raise(EventArgs args) => Moq.Raise.Event(args);

        /// <summary>
        /// Raises the event being attached to, passing the target mock 
        /// as the sender, and the given <paramref name="args"/> as the 
        /// event arguments.
        /// </summary>
        public static TEventHandler Raise<TEventHandler>(EventArgs args) => Moq.Raise.Event<TEventHandler>(args);

        /// <summary>
        /// Raises the event being attached to, passing the given 
        /// <paramref name="args"/> as the event arguments.
        /// </summary>
        public static TEventHandler Raise<TEventHandler>(params object[] args) => Moq.Raise.Event<TEventHandler>(args);

        /// <summary>
        /// Marks a code block as being setup for mocks. Usage: <c>using (Setup()) { ... }</c>.
        /// </summary>
        public static IDisposable Setup() => new SetupScope();
    }
}
