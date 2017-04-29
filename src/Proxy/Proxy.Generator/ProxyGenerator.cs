using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Moq.Proxy.Discovery;
using Moq.Proxy.Properties;

namespace Moq.Proxy
{
    /// <summary>
    /// Generates proxy classes for the given input symbols.
    /// </summary>
    class ProxyGenerator
    {
        // Used for MEF composition.
        // TODO: allow extending the codegen process to inject stuff into generated proxies via Roslyn.
        public static HostServices CreateHost() => Roslynator.CreateHost(Assembly.GetExecutingAssembly());

        /// <summary>
        /// Generates proxies by discovering proxy factory method invocations in the given 
        /// source documents.
        /// </summary>
        /// <param name="languageName">The language name to generate code for, such as 'C#' or 'Visual Basic'. See <see cref="LanguageNames"/>.</param>
        /// <param name="references">The metadata references to use when analyzing the <paramref name="sources"/>.</param>
        /// <param name="sources">The source documents to analyze to discover proxy usage.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the generation process.</param>
        /// <returns>An immutable array of the generated proxies as <see cref="Document"/> instances.</returns>
        public Task<ImmutableArray<Document>> GenerateProxiesAsync(
            string languageName,
            ImmutableArray<string> references,
            ImmutableArray<string> sources,
            ImmutableArray<string> additionalInterfaces,
            CancellationToken cancellationToken) => GenerateProxiesAsync(new AdhocWorkspace(CreateHost()), languageName, references, sources, additionalInterfaces, cancellationToken);

        /// <summary>
        /// Generates proxies by discovering proxy factory method invocations in the given 
        /// source documents.
        /// </summary>
        /// <param name="workspace">Creating a workspace is typically a bit heavy because of the MEF discovery, 
        /// so this argument allows reusing a previously created one across multiple calls.</param>
        /// <param name="languageName">The language name to generate code for, such as 'C#' or 'Visual Basic'. See <see cref="LanguageNames"/>.</param>
        /// <param name="references">The metadata references to use when analyzing the <paramref name="sources"/>.</param>
        /// <param name="sources">The source documents to analyze to discover proxy usage.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the generation process.</param>
        /// <returns>An immutable array of the generated proxies as <see cref="Document"/> instances.</returns>
        public async Task<ImmutableArray<Document>> GenerateProxiesAsync(
            AdhocWorkspace workspace,
            string languageName,
            ImmutableArray<string> references,
            ImmutableArray<string> sources,
            ImmutableArray<string> additionalInterfaces,
            CancellationToken cancellationToken)
        {
            var options = languageName == LanguageNames.CSharp ?
                    (CompilationOptions)new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) :
                    (CompilationOptions)new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            var project = workspace.AddProject(ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "pgen",
                "pgen.dll",
                languageName,
                compilationOptions: options,
                metadataReferences: references
                    .Select(path => MetadataReference.CreateFromFile(path))));

            foreach (var source in sources)
            {
                var document = workspace.AddDocument(DocumentInfo.Create(
                    DocumentId.CreateNewId(project.Id),
                    Path.GetFileName(source),
                    filePath: Path.GetTempFileName(),
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From(File.ReadAllText(source)), VersionStamp.Create()))));

                project = document.Project;
            }

            var compilation = await project.GetCompilationAsync();
            var additionalSymbols = new List<ITypeSymbol>();
            foreach (var additionalInterface in additionalInterfaces)
            {
                var additionalSymbol = compilation.GetTypeByMetadataName(additionalInterface) ??
                    // TODO: improve reporting
                    throw new ArgumentException(additionalInterface);

                additionalSymbols.Add(additionalSymbol);
            }

            var discoverer = new ProxyDiscoverer();
            var proxies = await discoverer.DiscoverProxiesAsync(project, cancellationToken);

            var documents = new List<Document>(proxies.Count);
            foreach (var proxy in proxies)
            {
                documents.Add(await GenerateProxyAsync(workspace, project, cancellationToken, proxy.AddRange(additionalSymbols)));
            }

            return documents.ToImmutableArray();
        }

        /// <summary>
        /// Generates a proxy that implements the given interfaces.
        /// </summary>
        /// <param name="workspace">Workspace in use for the code generation.</param>
        /// <param name="project">Code generation project.</param>
        /// <param name="cancellationToken">Cancellation token to abort the code generation process.</param>
        /// <param name="types">Base type (optional) and base interfaces the proxy should implement.</param>
        /// <returns>A <see cref="Document"/> containing the proxy code.</returns>
        public Task<Document> GenerateProxyAsync(AdhocWorkspace workspace, Project project,
            CancellationToken cancellationToken, params ITypeSymbol[] types)
            => GenerateProxyAsync(workspace, project, cancellationToken, types.ToImmutableArray());

        /// <summary>
        /// Generates a proxy that implements the given interfaces.
        /// </summary>
        /// <param name="workspace">Workspace in use for the code generation.</param>
        /// <param name="project">Code generation project.</param>
        /// <param name="cancellationToken">Cancellation token to abort the code generation process.</param>
        /// <param name="types">Base type (optional) and base interfaces the proxy should implement.</param>
        /// <returns>A <see cref="Document"/> containing the proxy code.</returns>
        public async Task<Document> GenerateProxyAsync(AdhocWorkspace workspace, Project project,
            CancellationToken cancellationToken, ImmutableArray<ITypeSymbol> types)
        {
            // TODO: the project *must* have a reference to the Moq.Proxy assembly. How do we verify that?

            // TODO: Sort interfaces so regardless of order, we reuse the proxies?
            //if (types.FirstOrDefault()?.TypeKind == TypeKind.Interface)
            //    Array.Sort(types, Comparer<INamedTypeSymbol>.Create((x, y) => x.Name.CompareTo(y.Name)));
            //else if (types.Length > 1)
            //    Array.Sort(types, 1, types.Length - 1, Comparer<INamedTypeSymbol>.Create((x, y) => x.Name.CompareTo(y.Name)));

            cancellationToken.ThrowIfCancellationRequested();

            var (baseType, interfaceTypes) = ValidateTypes(types);

            var generator = SyntaxGenerator.GetGenerator(project);
            var name = "ProxyOf" + string.Join("", types.Select(x => x.Name));

            var syntax = generator.CompilationUnit(types
                .Where(x => x.ContainingNamespace != null && x.ContainingNamespace.CanBeReferencedByName)
                .Select(x => x.ContainingNamespace.ToDisplayString())
                .Concat(new[]
                {
                    typeof(EventArgs).Namespace,
                    typeof(IList<>).Namespace,
                    typeof(MethodBase).Namespace,
                    typeof(IProxy).Namespace,
                })
                .Distinct()
                .Select(x => generator.NamespaceImportDeclaration(x))
                .Concat(new[]
                {
                    generator.ClassDeclaration(name,
                        accessibility: Accessibility.Public,
                        baseType: baseType == null ? null : generator.IdentifierName(baseType.Name),
                        interfaceTypes: interfaceTypes.Select(x => generator.IdentifierName(x.Name))
                            // NOTE: we *always* append IProxy at the end, which is what we use 
                            // in ImplementInterfaces to determine that it must *always* be implemented 
                            // explicitly.
                            .Concat(new[] { generator.IdentifierName(nameof(IProxy)) }))
                }));
                
            var services = workspace.Services.GetService<ICodeAnalysisServices>();
            var document = workspace.AddDocument(DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                name,
                filePath: Path.GetTempFileName(),
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(syntax.NormalizeWhitespace().ToFullString()), VersionStamp.Create()))));

            var scaffolds = services.GetLanguageServices<IDocumentVisitor>(project.Language, GeneratorLayer.Scaffold).ToArray();
            foreach (var scaffold in scaffolds)
            {
                document = await scaffold.VisitAsync(document, cancellationToken);
            }

            var rewriters = services.GetLanguageServices<IDocumentVisitor>(project.Language, GeneratorLayer.Rewrite).ToArray();
            foreach (var rewriter in rewriters)
            {
                document = await rewriter.VisitAsync(document, cancellationToken);
            }

            return document;
        }

        (ITypeSymbol baseType, ImmutableArray<ITypeSymbol> interfaceTypes) ValidateTypes(ImmutableArray<ITypeSymbol> types)
        {
            var baseType = default(ITypeSymbol);
            var interfaceTypes = default(ImmutableArray<ITypeSymbol>);
            if (types[0].TypeKind == TypeKind.Class)
            {
                baseType = types[0];
                interfaceTypes = types.Skip(1).ToImmutableArray();
            }
            else
            {
                interfaceTypes = types;
            }

            if (interfaceTypes.Any(x => x.TypeKind == TypeKind.Class))
                throw new ArgumentException(Strings.WrongProxyBaseType(string.Join(",", types.Select(x => x.Name))));
            if (interfaceTypes.Any(x => x.TypeKind != TypeKind.Interface))
                throw new ArgumentException(Strings.InvalidProxyTypes(string.Join(",", types.Select(x => x.Name))));

            return (baseType, interfaceTypes);
        }
   }
}