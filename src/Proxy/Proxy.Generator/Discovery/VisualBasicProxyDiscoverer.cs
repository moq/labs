using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Discovery
{
    [ExportLanguageService(typeof(IProxyDiscoverer), LanguageNames.VisualBasic)]
    class VisualBasicProxyDiscoverer : IProxyDiscoverer
    {
        public Task<ImmutableHashSet<ImmutableArray<ITypeSymbol>>> DiscoverProxiesAsync(Document document, ITypeSymbol proxyGeneratorSymbol, CancellationToken cancellationToken = default(CancellationToken))
            => document.DiscoverProxiesAsync<MemberAccessExpressionSyntax>(proxyGeneratorSymbol, cancellationToken);
    }
}