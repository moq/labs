using System;
using System.ComponentModel;
using System.Text;
using Microsoft.CodeAnalysis;
using Mono.Cecil;

namespace Stunts.Emit
{
    /// <summary>
    /// Provides <c>ToFullName</c> extension methods.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SymbolFullNameExtensions
    {
        static readonly SymbolDisplayFormat fullNameFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable);

        /// <summary>
        /// Gets the full name for the symbol.
        /// </summary>
        public static string ToFullName(this ITypeSymbol symbol)
        {
            // TODO: this method could leverage ReadOnlySpan<char> if available.
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
        /// Gets the full name for the symbol.
        /// </summary>
        public static string ToFullName(this TypeReference reference)
        {
            return reference.FullName;
        }
    }
}
