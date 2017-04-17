using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy
{
    static class DocumentExtensions
    {
        public static async Task<ImmutableHashSet<ImmutableArray<ITypeSymbol>>> DiscoverProxiesAsync<TSyntax>(this Document document, ITypeSymbol proxyGeneratorSymbol, CancellationToken cancellationToken = default(CancellationToken))
            where TSyntax : SyntaxNode
        {
            var proxies = new HashSet<ImmutableArray<ITypeSymbol>>(StructuralComparer<ImmutableArray<ITypeSymbol>>.Default);
            var semantic = await document.GetSemanticModelAsync(cancellationToken);
            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var node in syntax.DescendantNodes().OfType<TSyntax>())
            {
                var symbol = semantic.GetSymbolInfo(node, cancellationToken);
                if (symbol.Symbol?.Kind == SymbolKind.Method)
                {
                    var method = (IMethodSymbol)symbol.Symbol;
                    if (method.GetAttributes().Any(x => x.AttributeClass == proxyGeneratorSymbol) &&
                        // Skip generic method definitions since they are typically usability overloads 
                        // like Mock.Of<T>(...)
                        !method.TypeArguments.Any(x => x.Kind == SymbolKind.TypeParameter))
                        proxies.Add(method.TypeArguments);
                }
            }

            return proxies.ToImmutableHashSet(StructuralComparer<ImmutableArray<ITypeSymbol>>.Default);
        }
    }
}
