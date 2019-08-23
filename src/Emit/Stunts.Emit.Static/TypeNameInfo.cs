using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Mono.Cecil;
using Superpower;
using Superpower.Parsers;

namespace Stunts.Emit.Static
{
    public class TypeNameInfo
    {
        static readonly SymbolDisplayFormat fullNameFormat =
            new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                // genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable);

        public TypeNameInfo(string fullName, IEnumerable<TypeNameInfo> genericArguments, int arrayRank)
        {
            FullName = fullName;
            GenericArguments = genericArguments.ToArray();
            ArrayRank = arrayRank;
        }

        public AssemblyName AssemblyName { get; set; }

        public string FullName { get; set; }

        public TypeNameInfo[] GenericArguments { get; set; }

        public int ArrayRank { get; set; }

        public bool IsTypeParameter { get; set; }

        public string ToDisplayName()
        {
            var builder = new StringBuilder();
            ToDisplayName(builder, this);
            return builder.ToString();
        }

        public string ToMetadataName()
        {
            var builder = new StringBuilder();
            ToMetadataName(builder, this);
            return builder.ToString();
        }

        public override string ToString() => ToDisplayName();

        public static TypeNameInfo FromSymbol(ITypeSymbol symbol)
        {
            var fullName = symbol.ToDisplayString(fullNameFormat);
            var arrayRank = 0;
            if (symbol is IArrayTypeSymbol array)
            {
                arrayRank = array.Rank;
                fullName = fullName.Substring(0, fullName.IndexOf('['));
                symbol = array.ElementType;
            }

            var arguments = Array.Empty<TypeNameInfo>();
            if (symbol is INamedTypeSymbol named && named.TypeArguments.Length > 0)
            {
                arguments = named.TypeArguments.Select(arg => FromSymbol(arg)).ToArray();
                //fullName = fullName.Substring(0, fullName.IndexOf('`'));
            }

            return new TypeNameInfo(fullName, arguments, arrayRank) { IsTypeParameter = symbol.Kind == SymbolKind.TypeParameter };
        }

        public static TypeNameInfo FromReference(TypeReference reference)
        {
            // We don't consider the trailing '&' in this case, since we just care 
            // about locating types, not how they are passed around.
            if (reference.IsByReference && reference is ByReferenceType byRef)
                reference = byRef.ElementType;

            var nameBuilder = new StringBuilder(reference.Name);
            var currentType = reference;
            while (currentType.DeclaringType != null)
            {
                nameBuilder.Insert(0, currentType.DeclaringType.Name + ".");
                currentType = currentType.DeclaringType;
            }

            if (!string.IsNullOrEmpty(currentType.Namespace))
                nameBuilder.Insert(0, currentType.Namespace + ".");

            var fullName = nameBuilder.ToString();
            var arguments = Array.Empty<TypeNameInfo>();
            var arrayRank = 0;

            if (reference is ArrayType array)
            {
                arrayRank = array.Rank;
                fullName = fullName.Substring(0, fullName.IndexOf('['));
            }

            if (reference is GenericInstanceType generic)
            {
                fullName = fullName.Substring(0, fullName.IndexOf('`'));
                arguments = generic.GenericArguments.Select(arg => FromReference(arg)).ToArray();
            }

            return new TypeNameInfo(fullName, arguments, arrayRank) { IsTypeParameter = reference.IsGenericParameter };
        }

        public static TypeNameInfo FromMetadataName(string metadataName) => TypeNameInfoParser.Parse(metadataName);

        // Can't have implicit conversion from INamedTypeSymbol because it's an interface :(

        public static implicit operator TypeNameInfo(TypeReference reference) => FromReference(reference);

        public static implicit operator TypeNameInfo(string metadataName) => FromMetadataName(metadataName);

        static string ToDisplayName(StringBuilder builder, TypeNameInfo info)
        {
            builder.Append(info.FullName);

            if (info.GenericArguments.Length != 0)
            {
                // builder.Append('`').Append(info.Arity);
                builder.Append('<');
                var first = true;
                foreach (var arg in info.GenericArguments)
                {
                    if (!first)
                        builder.Append(", ");
                    else
                        first = false;

                    ToDisplayName(builder, arg);
                }
                builder.Append('>');
            }

            if (info.ArrayRank != 0)
            {
                builder.Append('[').Append(new string(',', info.ArrayRank - 1)).Append(']');
            }

            return builder.ToString();
        }

        static string ToMetadataName(StringBuilder builder, TypeNameInfo info)
        {
            builder.Append(info.FullName);

            if (info.GenericArguments.Length != 0)
            {
                builder.Append('`').Append(info.GenericArguments.Length).Append('[');
                var first = true;
                foreach (var arg in info.GenericArguments)
                {
                    if (!first)
                        builder.Append(',');
                    else
                        first = false;

                    builder.Append('[');
                    ToMetadataName(builder, arg);
                    builder.Append(']');
                }
                builder.Append(']');
            }

            if (info.ArrayRank > 0)
            {
                builder.Append('[').Append(new string(',', info.ArrayRank - 1)).Append(']');
            }

            if (info.AssemblyName != null)
                builder.Append(", ").Append(info.AssemblyName.FullName);

            return builder.ToString();
        }

        /// <summary>
        /// Adapted from https://github.com/nblumhardt/whitebox/blob/dev/Whitebox.Core/Util/TypeNameParser.cs
        /// </summary>
        static class TypeNameInfoParser
        {
            static readonly TextParser<char> Comma = Character.EqualTo(',');

            static readonly TextParser<char> TypeSimpleNameChar =
                Character.LetterOrDigit.Or(Character.Matching(c => "._-<>".Contains(c), "type name char"));

            // NOTE: we parse dim separators
            static TextParser<int> ArrayRank { get; } =
                (from open in Character.EqualTo('[')
                 // When constructing an array from reflection API, '*' is used to denote single dimension array
                 // Example: 
                 // typeof(int).MakeArrayType(1).FullName = "System.Int32[*]"
                 // typeof(int[]).FullName                = "System.Int32[]"
                 // typeof(int).MakeArrayType(1).IsAssignableFrom(typeof(int[])) = true
                 // Strangely, the converse is not true:
                 // typeof(int[]).IsAssignableFrom(typeof(int).MakeArrayType(1)) = true
                 from _ in Character.EqualTo('*').Optional()
                 from rank in Character.EqualTo(',').Many()
                 from close in Character.EqualTo(']')
                 select rank)
                .Many()
                .Log("rank")
                .Select(x => x.Length == 0 ? 0 :
                    x.Length == 1 ? x[0].Length + 1 : 
                    // Parsing gets complicated in this case, and I'm not sure it's worth it, since I 
                    // think most mock-able types are services and DTOs, not complex arrays. 
                    // We'll add support if it becomes a popular demand, of course.
                    throw new NotSupportedException("Jagged arrays are not supported.")
                );

            static readonly TextParser<char> AssemblyNameChar =
                Character.LetterOrDigit.Or(Character.EqualTo('.'));

            public static readonly TextParser<string> TypeSimpleName =
                TypeSimpleNameChar.AtLeastOnce().Select(c => new string(c));

            static readonly TextParser<string> AssemblyName =
                AssemblyNameChar.AtLeastOnce().Select(c => new string(c));

            static readonly TextParser<string> NestedTypeName =
                Character.EqualTo('+').Then(_ => TypeSimpleName);

            static readonly TextParser<TypeNameInfo> GenericArgument =
                from openDelim in Character.EqualTo('[')
                from argTypeName in Superpower.Parse.Ref(() => CompleteTypeName)
                from closeDelim in Character.EqualTo(']')
                select argTypeName;

            static readonly TextParser<IEnumerable<TypeNameInfo>> GenericArgumentList =
                from openDelimiter in Character.EqualTo('[')
                from firstArg in GenericArgument
                from remainingArgs in Comma.Token().Then(_ => GenericArgument).Many()
                from closeDelimiter in Character.EqualTo(']')
                select new[] { firstArg }.Concat(remainingArgs);

            static TextParser<T> Attribute<T>(string name, TextParser<T> value)
                 where T : class
            {
                return
                    (from leadingComma in Comma.Token()
                     from n in Span.EqualTo(name)
                     from equ in Character.EqualTo('=')
                     from val in value
                     select val).Or(Superpower.Parse.Return(default(T)));
            }

            static readonly TextParser<int> Integer = Numerics.Integer.Select(x => int.Parse(x.ToStringValue()));

            static readonly TextParser<char> Dot = Character.EqualTo('.');

            static readonly TextParser<Version> Version =
                from major in Integer
                from dot1 in Dot
                from minor in Integer
                from dot2 in Dot
                from build in Integer
                from dot3 in Dot
                from rev in Integer
                select new Version(major, minor, build, rev);

            static readonly TextParser<string> Culture =
                Character.LetterOrDigit.Or(Character.EqualTo('-')).AtLeastOnce().Select(c => new string(c));

            static readonly TextParser<string> PublicKeyToken =
                Character.LetterOrDigit.AtLeastOnce().Select(c => new string(c));

            public static readonly TextParser<int> GenericArgumentCount =
                from backtick in Character.EqualTo('`')
                from nargs in Character.Digit.AtLeastOnce().Select(c => new string(c))
                select int.Parse(nargs);

            static readonly TextParser<TypeNameInfo> CompleteTypeName =
                from simpleName in TypeSimpleName.Log(nameof(TypeSimpleName))
                from arity in GenericArgumentCount.Log(nameof(GenericArgumentCount)).OptionalOrDefault()
                from arguments in GenericArgumentList.Log(nameof(GenericArgumentList)).Try().OptionalOrDefault(Enumerable.Empty<TypeNameInfo>())
                from nestedTypeName in NestedTypeName.Log(nameof(NestedTypeName)).OptionalOrDefault("")
                from rank in ArrayRank.Log(nameof(ArrayRank)).OptionalOrDefault()
                from assemblyName in
                    (from comma in Comma.Log(nameof(Comma)).Token()
                     from assembly in AssemblyName.Log(nameof(AssemblyName))
                     from version in Attribute("Version", Version).Log("Version").OptionalOrDefault()
                     from culture in Attribute("Culture", Culture).Log("Culture").OptionalOrDefault()
                     from publicKeyToken in Attribute("PublicKeyToken", PublicKeyToken).Log("PublicKeyToken").OptionalOrDefault()
                     select assembly != null && version != null && culture != null && publicKeyToken != null ?
                        new AssemblyName(assembly + ", Version=" + version + " , Culture=" + culture + ", PublicKeyToken=" + publicKeyToken) :
                        null).OptionalOrDefault()
                let fullName = simpleName + (nestedTypeName == "" ? "" : ("+" + nestedTypeName))
                select new TypeNameInfo(fullName, arguments, rank) { AssemblyName = assemblyName };

            public static TypeNameInfo Parse(string assemblyQualifiedTypeName)
                => CompleteTypeName.Parse(assemblyQualifiedTypeName ?? throw new ArgumentNullException(nameof(assemblyQualifiedTypeName)));
        }
    }
}
