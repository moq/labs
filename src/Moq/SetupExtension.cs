using System;
using System.ComponentModel;

namespace Moq
{
    /// <summary>
    /// Extension for initiating a setup block, meaning 
    /// invocation tracking and strict mock behavior should 
    /// be suspended.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SetupExtension
    {
        /// <summary>
        /// Marks a code block as being setup for mocks (any, not just the 
        /// <paramref name="mock"/> argument). Usage: <c>using (mock.Setup()) { ... }</c>.
        /// </summary>
        /// <remarks>
        /// Even though this is an extension method on a particular instance of a mock, the scope 
        /// is active and affects all invocations to any mocks called within the block.
        /// </remarks>
        /// <seealso cref="SetupScope"/>
        public static IDisposable Setup<T>(this T mock) => new SetupScope();

        /// <summary>
        /// Sets up the mock with the given void method call.
        /// </summary>
        public static ISetup Setup<T>(this T mock, Action<T> action)
        {
            using (new SetupScope())
            {
                action(mock);
                return DefaultSetup.Default;
            }
        }

        /// <summary>
        /// Sets up the mock with the given function.
        /// </summary>
        public static TResult Setup<T, TResult>(this T mock, Func<T, TResult> function)
        {
            using (new SetupScope())
            {
                return function(mock);
            }
        }

        /// <summary>
        /// Sets up the mock with the given method reference, typically used to 
        /// access and set ref/out arguments. A code fix will automatically 
        /// generate a delegate with the right signature when using this overload.
        /// </summary>
        public static ISetup<TDelegate> Setup<TDelegate>(this object mock, TDelegate member)
            => new DefaultSetup<TDelegate>(member as Delegate ?? throw new ArgumentException(ThisAssembly.Strings.Setup.DelegateExpected));

        /// <summary>
        /// Sets up the mock with the given method reference, typically used to 
        /// access and set ref/out arguments. Use this overload when there is 
        /// a recursive mock involved. For a direct method of the mock, you can 
        /// use the <see cref="Setup{TDelegate}(object, TDelegate)"/> overload 
        /// and pass in the method group directly instead. A code fix will automatically 
        /// generate a delegate with the right signature when using this overload.
        /// </summary>
        public static ISetup<TDelegate> Setup<TDelegate>(this object mock, Func<TDelegate> memberFunction)
        {
            using (new SetupScope())
            {
                return new DefaultSetup<TDelegate>(memberFunction() as Delegate ?? throw new ArgumentException(ThisAssembly.Strings.Setup.DelegateExpected));
            }
        }

        private class DefaultSetup<TDelegate> : ISetup<TDelegate>
        {
            public DefaultSetup(Delegate @delegate) => Delegate = @delegate;

            public Delegate Delegate { get; }
        }

        private class DefaultSetup : ISetup
        {
            public static ISetup Default { get; } = new DefaultSetup();

            private DefaultSetup() { }
        }
    }
}