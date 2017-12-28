using System;
using System.ComponentModel;
using Moq.Properties;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Extension for marking a mock as being set up, meaning 
    /// invocation tracking should be suspended.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class SetupExtension
    {
        /// <summary>
        /// Marks the mock as being set up, meaning 
        /// invocation tracking and strict behavior 
        /// should be suspended until the returned <see cref="IDisposable"/> 
        /// is disposed.
        /// </summary>
        /// <param name="mock"></param>
        /// <returns></returns>
        public static IDisposable Setup(this object mock)
        {
            var target = mock is IMocked mocked ?
                mocked.Mock : throw new ArgumentException(Resources.TargetNotMocked);

            return new SetupDisposable(target);
        }

        public static ISetup Setup<T>(this T mock, Action<T> action)
        {
            using (Setup(mock))
            {
                action(mock);
                return new SetupAdapter(MockSetup.Current);
            }
        }

        class SetupDisposable : IDisposable
        {
            IMock mock;

            public SetupDisposable(IMock mock)
            {
                this.mock = mock;
                mock.State.TryAdd<bool?>(KnownStates.Setup, true);
            }

            public void Dispose() => mock.State.TryRemove<bool?>(KnownStates.Setup, out var _);
        }
    }

    class SetupAdapter : ISetup, IFluentInterface
    {
        public SetupAdapter(IMockSetup setup) => Setup = setup;

        internal IMockSetup Setup { get; }
    }

    public interface ISetup
    {
    }
}

namespace Moq.Sdk
{
    public static class ISetupExtension
    {
        public static IMockSetup Sdk(this ISetup setup) => (setup as SetupAdapter)?.Setup;
    }
}