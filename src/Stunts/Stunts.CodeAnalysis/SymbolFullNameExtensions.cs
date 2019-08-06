using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Superpower;
using Superpower.Parsers;

namespace Stunts
{
    /// <summary>
    /// Provides uniform rendering and resolving of symbols from a full name.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SymbolFullNameExtensions
    {
        static readonly SymbolDisplayFormat fullNameFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable);

        /// <summary>
        /// Gets the full name for the symbol, which then be used with 
        /// <see cref="GetTypeByFullName(Compilation, string)"/> to resolve it 
        /// back to the original symbol.
        /// </summary>
        public static string ToFullName(this ITypeSymbol symbol) => symbol.ToDisplayString(fullNameFormat);

        /// <summary>
        /// Resolves a symbol given its full name, as returned by <see cref="ToFullName(ITypeSymbol)"/>.
        /// </summary>
        public static ITypeSymbol GetTypeByFullName(this Compilation compilation, string symbolFullName)
            => new SymbolResolver(compilation).Resolve(symbolFullName ?? throw new ArgumentNullException(nameof(symbolFullName)));

        class SymbolResolver
        {
            TextParser<ITypeSymbol[]> symbolArguments;
            TextParser<ITypeSymbol> typeSymbol;
            Compilation compilation;

            public SymbolResolver(Compilation compilation) => this.compilation = compilation;

            static TextParser<string> Identifier { get; } =
                from first in Character.Letter
                from rest in Character.LetterOrDigit.Or(Character.EqualTo('_')).Many()
                select first + new string(rest);

            static TextParser<int> ArrayRank { get; } =
                from open in Character.EqualTo('[')
                from dimensions in Character.EqualTo(',').Many()
                from close in Character.EqualTo(']')
                select dimensions.Length + 1;

            static TextParser<string> FullName { get; } =
                from identifiers in Identifier.ManyDelimitedBy(Character.EqualTo('.').Or(Character.EqualTo('+')))
                select string.Join(".", identifiers);

            TextParser<ITypeSymbol[]> SymbolArguments => LazyInitializer.EnsureInitialized(ref symbolArguments, () =>
                from open in Character.EqualTo('<')
                from arguments in TypeSymbol.ManyDelimitedBy(Character.EqualTo(',').IgnoreThen(Character.WhiteSpace))
                from close in Character.EqualTo('>')
                select arguments);

            TextParser<ITypeSymbol> TypeSymbol => LazyInitializer.EnsureInitialized(ref typeSymbol, () =>
                from name in FullName
                from dimensions in ArrayRank.OptionalOrDefault()
                from arguments in SymbolArguments.OptionalOrDefault(Array.Empty<ITypeSymbol>())
                select ResolveSymbol(name, dimensions, arguments));

            public ITypeSymbol Resolve(string typeName) => TypeSymbol.Parse(typeName);

            ITypeSymbol ResolveSymbol(string fullName, int arrayRank, ITypeSymbol[] typeArguments)
            {
                var metadataName = fullName;
                if (typeArguments.Length > 0)
                    metadataName += "`" + typeArguments.Length;

                var symbol = compilation.GetTypeByMetadataName(metadataName);
                if (symbol == null)
                {
                    var nameBuilder = new StringBuilder(metadataName);
                    // Start replacing . with + to catch nested types, from 
                    // last to first
                    while (symbol == null)
                    {
                        var indexOfDot = nameBuilder.ToString().LastIndexOf('.');
                        if (indexOfDot == -1)
                            break;

                        nameBuilder[indexOfDot] = '+';
                        symbol = compilation.GetTypeByMetadataName(nameBuilder.ToString());
                    }
                }

                if (symbol == null)
                    return null;

                if (typeArguments.Length > 0)
                    symbol = symbol.Construct(typeArguments);

                if (arrayRank > 0 && symbol != null)
                    return compilation.CreateArrayTypeSymbol(symbol, arrayRank);

                return symbol;
            }
        }
    }
}
