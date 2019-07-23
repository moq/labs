using System;
using System.ComponentModel;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Provides mock instance verification extension methods.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class VerifyExtension
    {
        /// <summary>
        /// Verifies a method invocation matching the <paramref name="action"/> was executed the 
        /// given number of times.
        /// </summary>
        /// <param name="target">The mock instance to verify.</param>
        /// <param name="action">The method invocation to match against actual calls.</param>
        /// <param name="times">Number of times the method should have been called.</param>
        public static void Verify<T>(this T target, Action<T> action, int times)
            => Moq.Verify.Called(() => action(target), times);

        /// <summary>
        /// Verifies a method invocation matching the <paramref name="action"/> was executed at 
        /// least once.
        /// </summary>
        /// <param name="target">The mock instance to verify.</param>
        /// <param name="action">The method invocation to match against actual calls.</param>
        /// <param name="message">Optional user message to show.</param>
        public static void Verify<T>(this T target, Action<T> action, string message)
            => Moq.Verify.Called(() => action(target), message);

        /// <summary>
        /// Verifies a method invocation matching the <paramref name="action"/> was executed at 
        /// least once. If <paramref name="times"/> is provided, the number of calls is verified too.
        /// </summary>
        /// <param name="target">The mock instance to verify.</param>
        /// <param name="action">The method invocation to match against actual calls.</param>
        /// <param name="times">Optional number of times the method should have been called. Defaults to <see cref="Times.AtLeastOnce"/>. 
        /// An integer value can also be specificed since there is built-in conversion support from integer to <see cref="Times"/>.</param>
        /// <param name="message">Optional user message to show.</param>
        public static void Verify<T>(this T target, Action<T> action, int times = -1, string message = null)
            => Moq.Verify.CalledImpl(() => action(target), (Sdk.Times)times, message);

        /// <summary>
        /// Verifies a method invocation matching the <paramref name="function"/> was executed the 
        /// given number of times.
        /// </summary>
        /// <param name="target">The mock instance to verify.</param>
        /// <param name="function">The method invocation to match against actual calls.</param>
        /// <param name="times">Number of times the method should have been called.</param>
        public static void Verify<T, TResult>(this T target, Func<T, TResult> function, int times)
            => Moq.Verify.Called(() => function(target), times);

        /// <summary>
        /// Verifies a method invocation matching the <paramref name="function"/> was executed at 
        /// least once.
        /// </summary>
        /// <param name="target">The mock instance to verify.</param>
        /// <param name="function">The method invocation to match against actual calls.</param>
        /// <param name="message">Optional user message to show.</param>
        public static void Verify<T, TResult>(this T target, Func<T, TResult> function, string message)
            => Moq.Verify.Called(() => function(target), message);

        /// <summary>
        /// Verifies a method invocation matching the <paramref name="function"/> was executed at 
        /// least once. If <paramref name="times"/> is provided, the number of calls is verified too.
        /// </summary>
        /// <param name="target">The mock instance to verify.</param>
        /// <param name="function">The method invocation to match against actual calls.</param>
        /// <param name="times">Optional number of times the method should have been called. Defaults to <see cref="Times.AtLeastOnce"/>. 
        /// An integer value can also be specificed since there is built-in conversion support from integer to <see cref="Times"/>.</param>
        /// <param name="message">Optional user message to show.</param>
        public static void Verify<T, TResult>(this T target, Func<T, TResult> function, int times = -1, string message = null)
            => Moq.Verify.CalledImpl(() => function(target), (Sdk.Times)times, message);
    }
}
