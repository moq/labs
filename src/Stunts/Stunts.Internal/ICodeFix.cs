using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Stunts
{
    /// <summary>
    /// A code fix that can be applied to a <see cref="Workspace"/> for 
    /// the given set of <see cref="Diagnostics"/>.
    /// </summary>
    interface ICodeFix
    {
        /// <summary>
        /// The <see cref="CodeAction"/> that applies the fix. 
        /// </summary>
        CodeAction Action { get; }

        /// <summary>
        /// The diagnostics that can be fixed by the <see cref="Action"/>.
        /// </summary>
        ImmutableArray<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// The name of the <see cref="CodeFixProvider"/>, which can be retrieved
        /// using <see cref="ICodeFixService.GetCodeFixProvider"/>.
        /// </summary>
        string Provider { get; }
    }
}
