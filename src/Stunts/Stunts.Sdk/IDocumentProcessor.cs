using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Stunts
{
    /// <summary>
    /// Interface implemented by components that participate in stunt code generation.
    /// </summary>
    public interface IDocumentProcessor
    {
        /// <summary>
        /// Gets the language the visitor supports.
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Gets the phase at which an <see cref="IDocumentProcessor"/> acts.
        /// </summary>
        ProcessorPhase Phase { get; }

        /// <summary>
        /// Processes the stunt document and optionally modifies its source code.
        /// </summary>
        Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken));
    }
}