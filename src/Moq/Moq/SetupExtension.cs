using System;
using System.ComponentModel;
using Moq.Properties;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Extension for initiating a setup block, meaning 
    /// invocation tracking and strict mock behavior should 
    /// be suspended.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class SetupExtension
    {
        /// <summary>
        /// Sets up the mock with the given void method call.
        /// </summary>
        [SetupScope]
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
        [SetupScope]
        public static TResult Setup<T, TResult>(this T mock, Func<T, TResult> function)
        {
            using (new SetupScope())
            {
                var stunt = mock as IStunt ?? throw new ArgumentException(Strings.TargetNotMock, nameof(mock));
                return function(mock);
            }
        }

        class DefaultSetup : ISetup
        {
            public static ISetup Default { get; } = new DefaultSetup();

            DefaultSetup() { }
        }
    }
}