using Microsoft.CodeAnalysis.Options;

namespace Stunts
{
    /// <summary>
    /// Contains options that affect the code fixers.
    /// </summary>
    public class StuntOptions
    {
        /// <summary>
        /// Whether the workspace has been loaded for build-time code generation.
        /// </summary>
        public static Option<bool> IsBuildTime { get; } = new Option<bool>(nameof(Stunts), nameof(IsBuildTime), false);
    }
}
