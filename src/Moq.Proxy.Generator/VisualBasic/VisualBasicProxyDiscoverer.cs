using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.VisualBasic
{
    [ExportLanguageService(typeof(IProxyDiscoverer), LanguageNames.VisualBasic)]
    class VisualBasicProxyDiscoverer : VisualBasicSyntaxWalker, IProxyDiscoverer
    {
        List<ImmutableArray<ITypeSymbol>> mocks = new List<ImmutableArray<ITypeSymbol>>();
        Document document;
        ITypeSymbol proxyGeneratorSymbol;
        CancellationToken cancellationToken;

        SemanticModel semantic;

        public async Task<IReadOnlyList<ImmutableArray<ITypeSymbol>>> DiscoverProxiesAsync(Document document, ITypeSymbol proxyGeneratorSymbol, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.document = document;
            this.proxyGeneratorSymbol = proxyGeneratorSymbol;
            this.cancellationToken = cancellationToken;

            semantic = await document.GetSemanticModelAsync(cancellationToken);

            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            Visit(syntax);

            return mocks.AsReadOnly();
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var symbol = semantic.GetSymbolInfo(node, cancellationToken);
            if (symbol.Symbol?.Kind == SymbolKind.Method)
            {
                var method = (IMethodSymbol)symbol.Symbol;
                if (method.GetAttributes().Any(x => x.AttributeClass == proxyGeneratorSymbol))
                    mocks.Add(method.TypeArguments);
            }

            base.VisitMemberAccessExpression(node);
        }
    }
}
