using Microsoft.CodeAnalysis;
using Mono.Cecil;

namespace Stunts.Emit.Static
{
    public static class MetadataNameExtensions
    {
        public static string ToMetadataName(this ITypeSymbol symbol)
            => TypeNameInfo.FromSymbol(symbol).ToMetadataName();

        public static string ToMetadataName(this TypeReference reference)
            => TypeNameInfo.FromReference(reference).ToMetadataName();
    }
}
