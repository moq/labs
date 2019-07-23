#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.ComponentModel;
using static Moq.Syntax;

namespace Moq
{
    /// <summary>Supports the legacy API.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class It
    {
        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static T IsAny<T>() => Any<T>();
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member