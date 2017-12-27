using System;
using Moq.Sdk.Properties;

namespace Moq.Sdk
{
    /// <summary>
    /// Known states a mock can be in.
    /// </summary>
    public static class KnownStates
    {
        /// <summary>
        /// The key in <see cref="IMock.State"/> containing a <see cref="Nullable{Boolean}"/> 
        /// specifying if the mock is being set up or not.
        /// </summary>
        public const string Setup = nameof(KnownStates) + "." + nameof(Setup);

        /// <summary>
        /// Whether the mock is being set up.
        /// </summary>
        /// <param name="mock">The mock to inspect.</param>
        /// <returns><see langword="true"/> if the mock is being set up (it has the <see cref="Setup"/> state key 
        /// set to <see langword="true"/>), <see langword="false"/> otherwise.</returns>
        public static bool InSetup(object mock)
        {
            var target = mock is IMocked mocked ?
                mocked.Mock : throw new ArgumentException(Resources.TargetNotMocked, nameof(mock));

            return target.State.TryGetValue<bool?>(Setup, out var state) && state.GetValueOrDefault();
        }
    }
}
