using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using CSharp = Microsoft.CodeAnalysis.CSharp.Syntax;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Stunts.Properties;
using System.Linq;

namespace Stunts
{
    /// <summary>
    /// Extension methods on <see cref="Project"/> and <see cref="Document"/> to discover 
    /// stunts in use by detecting invocations to methods declared with the 
    /// <see cref="StuntGeneratorAttribute"/>.
    /// </summary>
    public static class DiscoveryExtensions
    {
        /// <summary>
        /// Discovers the unique set of stunts in use in the given project by searching all 
        /// documents for invocations to methods declared with the <see cref="StuntGeneratorAttribute"/>.
        /// </summary>
        /// <remarks>
        /// This method returns a set of arrays since each stunt is actually made of the symbols it 
        /// inherits/implements. It's a set because that array of symbols the stunt inherits/implements is 
        /// unique (we don't generate duplicates). A <see cref="StructuralComparer{T}.Default"/> is used to 
        /// ensure structural equality of the arrays.
        /// </remarks>
        public static async Task<IImmutableSet<ImmutableArray<INamedTypeSymbol>>> DiscoverStuntsAsync(this Project project, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compilation = await project.GetCompilationAsync(cancellationToken);
            var stuntGeneratorSymbol = compilation.GetTypeByMetadataName(typeof(StuntGeneratorAttribute).FullName);
            if (stuntGeneratorSymbol == null)
                throw new ArgumentException(Strings.StuntsRequired(project.Name));

            Func<Document, INamedTypeSymbol, CancellationToken, Task<ImmutableHashSet<ImmutableArray<INamedTypeSymbol>>>> discoverer;
            if (project.Language == LanguageNames.CSharp)
                discoverer = DiscoverStuntsAsync<CSharp.MemberAccessExpressionSyntax>;
            else if (project.Language == LanguageNames.VisualBasic)
                discoverer = DiscoverStuntsAsync<VisualBasic.MemberAccessExpressionSyntax>;
            else
                // TODO: F#
                return ImmutableHashSet.Create<ImmutableArray<INamedTypeSymbol>>();

            var candidates = new HashSet<ImmutableArray<INamedTypeSymbol>>(StructuralComparer<ImmutableArray<INamedTypeSymbol>>.Default);
            foreach (var document in project.Documents)
            {
                var discovered = await discoverer(document, stuntGeneratorSymbol, cancellationToken);
                foreach (var stunt in discovered)
                {
                    candidates.Add(stunt);
                }
            }

            return candidates.ToImmutableHashSet(StructuralComparer<ImmutableArray<INamedTypeSymbol>>.Default);
        }

        /// <summary>
        /// Discovers the unique set of stunts in use in the given document by searching for invocations 
        /// to methods declared with the <see cref="StuntGeneratorAttribute"/>.
        /// </summary>
        /// <summary>
        /// This method returns a set of arrays since each stunt is actually made of the symbols it 
        /// inherits/implements. It's a set because that array of symbols the stunt inherits/implements is 
        /// unique (we don't generate duplicates). A <see cref="StructuralComparer{T}.Default"/> is used to 
        /// ensure structural equality of the arrays.
        /// </summary>
        public static async Task<ImmutableHashSet<ImmutableArray<INamedTypeSymbol>>> DiscoverStuntsAsync(this Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compilation = await document.Project.GetCompilationAsync(cancellationToken);
            var stuntGeneratorSymbol = compilation.GetTypeByMetadataName(typeof(StuntGeneratorAttribute).FullName);
            if (stuntGeneratorSymbol == null)
                throw new ArgumentException(Strings.StuntsRequired(document.Project.Name));

            Func<Document, INamedTypeSymbol, CancellationToken, Task<ImmutableHashSet<ImmutableArray<INamedTypeSymbol>>>> discoverer;
            if (document.Project.Language == LanguageNames.CSharp)
                discoverer = DiscoverStuntsAsync<CSharp.MemberAccessExpressionSyntax>;
            else if (document.Project.Language == LanguageNames.VisualBasic)
                discoverer = DiscoverStuntsAsync<VisualBasic.MemberAccessExpressionSyntax>;
            else
                // TODO: F#
                return ImmutableHashSet.Create<ImmutableArray<INamedTypeSymbol>>();

            var discovered = await discoverer(document, stuntGeneratorSymbol, cancellationToken);

            return discovered.ToImmutableHashSet(StructuralComparer<ImmutableArray<INamedTypeSymbol>>.Default);
        }

        static async Task<ImmutableHashSet<ImmutableArray<INamedTypeSymbol>>> DiscoverStuntsAsync<TSyntax>(this Document document, ITypeSymbol proxyGeneratorSymbol, CancellationToken cancellationToken = default(CancellationToken))
            where TSyntax : SyntaxNode
        {
            var stunts = new HashSet<ImmutableArray<INamedTypeSymbol>>(StructuralComparer<ImmutableArray<INamedTypeSymbol>>.Default);
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
                        !method.TypeArguments.Any(x => x.Kind == SymbolKind.TypeParameter) &&
                        // TODO: this invalid scenario should be detected by an analyzer that 
                        // generates a compile error.
                        method.TypeArguments.All(x => x.TypeKind == TypeKind.Class || x.TypeKind == TypeKind.Interface))
                    {
                        var baseType = (INamedTypeSymbol)method.TypeArguments.FirstOrDefault(x => x.TypeKind == TypeKind.Class);
                        var implementedInterfaces = baseType != null ? method.TypeArguments.Remove(baseType) : method.TypeArguments;

                        stunts.Add(implementedInterfaces
                            .Cast<INamedTypeSymbol>()
                            .OrderBy(x => x.Name)
                            .Prepend(baseType)
                            .ToImmutableArray());
                    }
                }
            }

            return stunts.ToImmutableHashSet(StructuralComparer<ImmutableArray<INamedTypeSymbol>>.Default);
        }
    }
}
