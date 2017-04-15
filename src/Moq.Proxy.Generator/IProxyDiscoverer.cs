using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;

namespace Moq.Proxy
{
    public interface IProxyDiscoverer : ILanguageService
    {
        Task<IReadOnlyList<ImmutableArray<ITypeSymbol>>> DiscoverProxiesAsync(Document document, ITypeSymbol proxyGeneratorSymbol, CancellationToken cancellationToken = default(CancellationToken));
    }
}
