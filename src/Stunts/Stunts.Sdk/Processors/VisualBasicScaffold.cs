using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Stunts.Processors
{
    /// <summary>
    /// Applies a set of code fixes that scaffold empty implementations 
    /// of all abstract class, interface and overridable members.
    /// </summary>
    public class VisualBasicScaffold : IDocumentProcessor
    {
        /// <summary>
        /// Default code fixes when no specific fixes are provided. 
        /// </summary>
        static readonly string[] DefaultCodeFixNames = 
        {
            CodeFixNames.VisualBasic.ImplementAbstractClass,
            CodeFixNames.VisualBasic.ImplementInterface,
            "OverrideAllMembersCodeFix",
            CodeFixNames.VisualBasic.AddOverloads,
        };

        readonly string[] codeFixNames;

        /// <summary>
        /// Initializes the scaffold with the <see cref="DefaultCodeFixNames"/>.
        /// </summary>
        public VisualBasicScaffold() : this(DefaultCodeFixNames) { }

        /// <summary>
        /// Initializes the scaffold with a specific set of code fixes to apply.
        /// </summary>
        public VisualBasicScaffold(params string[] codeFixNames) => this.codeFixNames = codeFixNames;

        /// <summary>
        /// Applies to <see cref="LanguageNames.VisualBasic"/> only.
        /// </summary>
        public string[] Languages { get; } = new[] { LanguageNames.VisualBasic };

        /// <summary>
        /// Runs in the second phase of codegen, <see cref="ProcessorPhase.Scaffold"/>.
        /// </summary>
        public ProcessorPhase Phase => ProcessorPhase.Scaffold;

        /// <summary>
        /// Applies all existing code fixes to the document.
        /// </summary>
        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var codeFixName in codeFixNames)
            {
                document = await document.ApplyCodeFixAsync(codeFixName, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }
    }
}
