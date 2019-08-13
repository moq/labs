using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Stunts.Emit;
using Xunit;

namespace Emit.Tests
{
    public class StaticTypeResolverTests
    {
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(IEnumerable<int[,]>[]))]
        [InlineData(typeof(IEnumerable<KeyValuePair<string[], int[]>>))]
        [Theory]
        public void WhenResolvingSymbol_ThenCanResolveFromFullName(Type type)
        {
            var assemblyFile = Assembly.GetExecutingAssembly().Location;
            var resolver = new StaticTypeResolver(assemblyFile);

            var symbol = resolver.ResolveSymbol(type.FullName);

            Assert.NotNull(symbol);
        }

        [Fact]
        public void WhenResolvingFullName_ThenRoundtripsSymbolAndReference()
        {
            var assemblyFile = Assembly.GetExecutingAssembly().Location;
            var resolver = new StaticTypeResolver(assemblyFile);

            var reference = resolver.ResolveReference(typeof(IEnumerable<>).MakeGenericType(typeof(StaticTypeResolverTests).MakeArrayType(1)).AssemblyQualifiedName);

            Assert.NotNull(reference);

            var name = TypeNameInfo.FromReference(reference);

            var symbol = (INamedTypeSymbol)resolver.ResolveSymbol(reference);

            Assert.NotNull(symbol);

            Assert.Equal(TypeNameInfo.FromReference(reference).ToDisplayName(), TypeNameInfo.FromSymbol(symbol).ToDisplayName());

            Assert.NotNull(resolver.ResolveSymbol(typeof(StaticTypeResolverTests).FullName));

            var referenceFromSymbol = resolver.ResolveReference(symbol);

            Assert.Equal(reference.FullName, referenceFromSymbol.FullName);
        }
    }
}
