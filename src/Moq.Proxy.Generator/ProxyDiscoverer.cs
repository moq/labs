using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Moq.Proxy
{
    public class ProxyDiscoverer
    {
        public async Task<IImmutableSet<ImmutableArray<ITypeSymbol>>> DiscoverProxiesAsync(Project project, CancellationToken cancellationToken = default(CancellationToken))
        {
            var discoverer = project.LanguageServices.GetRequiredService<IProxyDiscoverer>();
            
            var compilation = await project.GetCompilationAsync(cancellationToken);
            var proxyGeneratorSymbol = compilation.GetTypeByMetadataName(typeof(ProxyGeneratorAttribute).FullName);

            // TODO: message.
            if (proxyGeneratorSymbol == null)
                throw new InvalidOperationException();

            var proxies = new HashSet<ImmutableArray<ITypeSymbol>>(StructuralComparer<ImmutableArray<ITypeSymbol>>.Default);
            foreach (var document in project.Documents)
            {
                var discovered = await discoverer.DiscoverProxiesAsync(document, proxyGeneratorSymbol, cancellationToken);
                foreach (var proxy in discovered)
                {
                    proxies.Add(proxy);
                }
            }
            
            return proxies.ToImmutableHashSet(StructuralComparer<ImmutableArray<ITypeSymbol>>.Default);
        }
    }
}
