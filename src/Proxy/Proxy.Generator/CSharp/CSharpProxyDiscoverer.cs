using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.CSharp
{
    [ExportLanguageService(typeof(IProxyDiscoverer), LanguageNames.CSharp)]
    class CSharpProxyDiscoverer : IProxyDiscoverer
    {
        public Task<ImmutableHashSet<ImmutableArray<ITypeSymbol>>> DiscoverProxiesAsync(Document document, ITypeSymbol proxyGeneratorSymbol, CancellationToken cancellationToken = default(CancellationToken))
            => document.DiscoverProxiesAsync<MemberAccessExpressionSyntax>(proxyGeneratorSymbol, cancellationToken);
    }
}
