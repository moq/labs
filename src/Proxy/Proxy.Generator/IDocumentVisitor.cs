using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy
{
    /// <summary>
    /// Provides a language service that can participate in code generation for 
    /// a proxy document. Must be exported using <see cref="ExportLanguageServiceAttribute"/> and 
    /// specifying one of the <see cref="DocumentVisitorLayer"/> phases to run.
    /// </summary>
    public interface IDocumentVisitor : ILanguageService
    {
        /// <summary>
        /// Visits the proxy document and optionally modifies its source code.
        /// </summary>
        Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken));
    }
}