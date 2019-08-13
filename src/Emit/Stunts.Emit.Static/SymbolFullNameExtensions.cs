using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Superpower;
using Superpower.Model;
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
        public static string ToFullName(this ITypeSymbol symbol)
        {
            // TODO: this method could leverage ReadOnlySpan<char> if we can use it.
            var fullName = symbol.ToDisplayString(fullNameFormat);
            var builder = new StringBuilder();
            var begin = fullName.IndexOf('[');
            if (begin == -1)
                return fullName;

            var current = 0;
            while (begin != -1)
            {
                builder.Append(fullName.Substring(current, begin));
                current = fullName.IndexOf(']', begin) + 1;
                if (current == 0)
                    throw new ArgumentException();

                var arr = fullName.Substring(begin, current - begin);
                builder.Append(arr.Replace(",", "]["));

                begin = fullName.IndexOf('[', current);
            }

            builder.Append(fullName.Substring(current));

            return builder.ToString();
        }

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

            static TextParser<int> Arity { get; } =
                (from begin in Character.EqualTo('`')
                 from count in Character.Numeric.Many()
                 select count).OptionalOrDefault(new[] { '0' }).Select(x => int.Parse(new string(x)));

            static TextParser<int> ArrayRank { get; } =
                (from open in Character.EqualTo('[')
                 from close in Character.EqualTo(']')
                 select Unit.Value).Many().Select(x => x.Length);

            static TextParser<string> FullName { get; } =
                from identifiers in Identifier.ManyDelimitedBy(Character.EqualTo('.').Or(Character.EqualTo('+')))
                select string.Join(".", identifiers);

            TextParser<ITypeSymbol[]> SymbolArguments => LazyInitializer.EnsureInitialized(ref symbolArguments, () =>
                from open in Character.EqualTo('<')
                from arguments in TypeSymbol.ManyDelimitedBy(Character.EqualTo(','))
                from close in Character.EqualTo('>')
                select arguments);

            TextParser<ITypeSymbol> TypeSymbol => LazyInitializer.EnsureInitialized(ref typeSymbol, () =>
                from name in FullName
                from arity in Arity
                from dimensions in ArrayRank.OptionalOrDefault()
                from arguments in SymbolArguments.OptionalOrDefault(Array.Empty<ITypeSymbol>())
                select ResolveSymbol(name, arity, dimensions, arguments));

            public ITypeSymbol Resolve(string typeName) => TypeSymbol.Parse(typeName);

            ITypeSymbol ResolveSymbol(string fullName, int arity, int arrayRank, ITypeSymbol[] typeArguments)
            {
                var metadataName = fullName;
                if (arity > 0)
                    metadataName += "`" + arity;

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
