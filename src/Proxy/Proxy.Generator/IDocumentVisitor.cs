using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;

namespace Moq.Proxy
{
    interface IDocumentVisitor : ILanguageService
    {
        Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken));
    }
}