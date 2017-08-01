using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Moq.Proxy.Discovery
{
    class ProxyDiscoverer
    {
        /// <summary>
        /// This method returns a set of arrays since each proxy is actually made of the symbols its type 
        /// inherits/implements. It's a set because that array of symbols the proxy inherits/implements is 
        /// unique (we don't generate duplicates). A <see cref="StructuralComparer.Default"/> is used to 
        /// ensure structural equality of the arrays.
        /// </summary>
        public async Task<IImmutableSet<ImmutableArray<ITypeSymbol>>> DiscoverProxiesAsync(Project project, CancellationToken cancellationToken = default(CancellationToken))
        {
            var discoverer = project.LanguageServices.GetRequiredService<IProxyDiscoverer>();
            
            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
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
