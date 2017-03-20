using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Moq.Proxy
{
    public class ProxySyntaxInfo
    {
        public ProxySyntaxInfo(params INamedTypeSymbol[] types)
        {
            // TODO: group by compatible method signatures, 
            // separate into declared and explicit members.
            ImplicitEvents = types.Concat(types.SelectMany(t => t.AllInterfaces)).SelectMany(t => t.GetMembers().OfType<IEventSymbol>());
            ImplicitMethods = types.Concat(types.SelectMany(t => t.AllInterfaces)).SelectMany(t => t.GetMembers().OfType<IMethodSymbol>());
            ImplicitProperties = types.Concat(types.SelectMany(t => t.AllInterfaces)).SelectMany(t => t.GetMembers().OfType<IPropertySymbol>());
        }

        public IEnumerable<IEventSymbol> ExplicitEvents { get; } = Enumerable.Empty<IEventSymbol>();
        public IEnumerable<IMethodSymbol> ExplicitMethods { get; } = Enumerable.Empty<IMethodSymbol>();
        public IEnumerable<IPropertySymbol> ExplicitProperties { get; } = Enumerable.Empty<IPropertySymbol>();

        public IEnumerable<IEventSymbol> ImplicitEvents { get; } = Enumerable.Empty<IEventSymbol>();
        public IEnumerable<IMethodSymbol> ImplicitMethods { get; } = Enumerable.Empty<IMethodSymbol>();
        public IEnumerable<IPropertySymbol> ImplicitProperties { get; } = Enumerable.Empty<IPropertySymbol>();
    }
}
