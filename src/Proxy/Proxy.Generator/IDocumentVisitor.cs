using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;

namespace Moq.Proxy
{
    /// <summary>
    /// Provides a language service that can participate in code generation for 
    /// a proxy document.
    /// </summary>
    interface IDocumentVisitor : ILanguageService
    {
        /// <summary>
        /// Visits the proxy document and optionally modifies its source code.
        /// </summary>
        Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken));
    }
}