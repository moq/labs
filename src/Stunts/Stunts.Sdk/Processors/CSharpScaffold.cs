using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Stunts.Processors
{
    /// <summary>
    /// Applies a set of code fixes that scaffold empty implementations 
    /// of all abstract class, interface and overridable members.
    /// </summary>
    public class CSharpScaffold : IDocumentProcessor
    {
        /// <summary>
        /// Default code fixes when no specific fixes are provided. 
        /// </summary>
        static readonly string[] DefaultCodeFixNames = 
        {
            CodeFixNames.CSharp.ImplementAbstractClass,
            CodeFixNames.CSharp.ImplementInterface,
            "OverrideAllMembersCodeFix",
        };

        readonly string[] codeFixNames;

        /// <summary>
        /// Initializes the scaffold with the <see cref="DefaultCodeFixNames"/>.
        /// </summary>
        public CSharpScaffold() : this(DefaultCodeFixNames) { }

        /// <summary>
        /// Initializes the scaffold with a specific set of code fixes to apply.
        /// </summary>
        public CSharpScaffold(params string[] codeFixNames) => this.codeFixNames = codeFixNames;

        /// <summary>
        /// Applies to <see cref="LanguageNames.CSharp"/> only.
        /// </summary>
        public string[] Languages { get; } = new[] { LanguageNames.CSharp };

        /// <summary>
        /// Runs in the second phase of codegen, <see cref="ProcessorPhase.Scaffold"/>.
        /// </summary>
        public ProcessorPhase Phase => ProcessorPhase.Scaffold;

        /// <summary>
        /// Applies all existing code fixes to the document.
        /// </summary>
        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var generator = SyntaxGenerator.GetGenerator(document);

            foreach (var codeFixName in codeFixNames)
            {
                document = await document.ApplyCodeFixAsync(codeFixName, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }
    }
}
