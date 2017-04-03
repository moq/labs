using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;

namespace Moq.Proxy
{
    public interface IDocumentRewriter : ILanguageService
    {
        Task<Document> VisitAsync(Document document, CancellationToken cancellationToken);
    }
}
