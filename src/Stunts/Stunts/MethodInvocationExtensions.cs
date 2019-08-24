using System;
using System.ComponentModel;

namespace Stunts
{
    /// <summary>
    /// Extension methods to detect various invocation patterns based on 
    /// the <see cref="IMethodInvocation"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MethodInvocationExtensions
    {
        /// <summary>
        /// Gets whether the invocation represents a property getter access.
        /// </summary>
        public static bool IsGetAccessor(this IMethodInvocation invocation)
            => invocation.MethodBase.IsSpecialName && invocation.MethodBase.Name.StartsWith("get_", StringComparison.Ordinal);

        /// <summary>
        /// Gets whether the invocation represents a property setter access.
        /// </summary>
        public static bool IsSetAccessor(this IMethodInvocation invocation)
            => invocation.MethodBase.IsSpecialName && invocation.MethodBase.Name.StartsWith("set_", StringComparison.Ordinal);

        /// <summary>
        /// Gets whether the invocation represents an indexer access.
        /// </summary>
        public static bool IsIndexerAccessor(this IMethodInvocation invocation)
        {
            var parameterCount = invocation.MethodBase.GetParameters().Length;
            return (invocation.IsGetAccessor() && parameterCount > 0)
                || (invocation.IsSetAccessor() && parameterCount > 1);
        }

        /// <summary>
        /// Gets whether the invocation represents a property access, either get or set.
        /// </summary>
        public static bool IsPropertyAccessor(this IMethodInvocation invocation)
        {
            var parameterCount = invocation.MethodBase.GetParameters().Length;
            return (invocation.IsGetAccessor() && parameterCount == 0)
                || (invocation.IsSetAccessor() && parameterCount == 1);
        }

        // NOTE: The following two methods used to first check whether `method.IsSpecialName` was set
        // as a quick guard against non-event accessor methods. This was removed in commit 44070a90
        // to "increase compatibility with F# and COM". More specifically:
        //
        //  1. COM does not really have events. Some COM interop assemblies define events, but do not
        //     mark those with the IL `specialname` flag. See:
        //      - https://code.google.com/archive/p/moq/issues/226
        //     - the `Microsoft.Office.Interop.Word.ApplicationEvents4_Event` interface in Office PIA
        //
        //  2. F# does not mark abstract events' accessors with the IL `specialname` flag. See:
        //      - https://github.com/Microsoft/visualfsharp/issues/5834
        //      - https://code.google.com/archive/p/moq/issues/238
        //      - the unit tests in `FSharpCompatibilityFixture`

        /// <summary>
        /// Gets whether the invocation represents an event subscription (+=).
        /// </summary>
        public static bool IsEventAddAccessor(this IMethodInvocation invocation)
            => invocation.MethodBase.Name.StartsWith("add_", StringComparison.Ordinal);

        /// <summary>
        /// Gets whether the invocation represents an event unsubscription (-=).
        /// </summary>
        public static bool IsEventRemoveAccessor(this IMethodInvocation invocation)
            => invocation.MethodBase.Name.StartsWith("remove_", StringComparison.Ordinal);

        /// <summary>
        /// Gets whether the invocation represents an event unsubscription (-=).
        /// </summary>
        public static bool IsEventAccessor(this IMethodInvocation invocation)
            => invocation.IsEventAddAccessor() || invocation.IsEventRemoveAccessor();
    }
}
