using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Moq.Proxy
{
    /// <summary>
    /// Generates proxy classes for the given input symbols.
    /// </summary>
    public class ProxyGenerator
    {
        // Used for MEF composition.
        public static HostServices CreateHost() => MefHostServices.Create(new ContainerConfiguration()
                .WithAssemblies(MefHostServices.DefaultAssemblies.Add(Assembly.GetExecutingAssembly()))
                .WithDefaultConventions(new AttributeFilterProvider())
                .CreateContainer());

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
            var project = workspace.AddProject("pgen", languageName)
                // TODO: would be nice to get these options directly from the project somehow, 
                // or set them by default to DynamicallyLinkedLibrary without requiring this?
                .WithCompilationOptions(languageName == LanguageNames.CSharp ?
                    (CompilationOptions)new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) :
                    (CompilationOptions)new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithMetadataReferences(references
                    .Select(x => MetadataReference.CreateFromFile(x)));

            foreach (var source in sources)
            {
                project = project.AddDocument(
                    Path.GetFileName(source), 
                    File.ReadAllText(source)
                ).Project;
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
        public Task<Document> GenerateProxyAsync(Workspace workspace, Project project,
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
        public async Task<Document> GenerateProxyAsync(Workspace workspace, Project project, 
            CancellationToken cancellationToken, ImmutableArray<ITypeSymbol> types)
        {
            // TODO: the project *must* have a reference to the Moq.Proxy assembly. How do we verify that?
            
            // TODO: Sort interfaces so regardless of order, we reuse the proxies?
            //if (types.FirstOrDefault()?.TypeKind == TypeKind.Interface)
            //    Array.Sort(types, Comparer<INamedTypeSymbol>.Create((x, y) => x.Name.CompareTo(y.Name)));
            //else if (types.Length > 1)
            //    Array.Sort(types, 1, types.Length - 1, Comparer<INamedTypeSymbol>.Create((x, y) => x.Name.CompareTo(y.Name)));

            cancellationToken.ThrowIfCancellationRequested();

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
                        interfaceTypes: types.Select(x => generator.IdentifierName(x.Name))
                            // NOTE: we *always* append IProxy at the end, which is what we use 
                            // in ImplementInterfaces to determine that it must *always* be implemented 
                            // explicitly.
                            .Concat(new[] { generator.IdentifierName(nameof(IProxy)) })) 
                }));

            var languageServices = workspace.Services.GetService<LanguageServiceRetriever>();
            var implementInterfaceService = languageServices.GetLanguageServices(project.Language,
                "Microsoft.CodeAnalysis.ImplementInterface.IImplementInterfaceService").FirstOrDefault();

            if (implementInterfaceService == null)
                // TODO: improve
                throw new NotSupportedException(project.Language);

            var getCodeActionsMethod = implementInterfaceService.GetType().GetMethod("GetCodeActions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            GetCodeActions getCodeActions = (d, s, n, t) => ((IEnumerable<CodeAction>)getCodeActionsMethod.Invoke(
                              implementInterfaceService,
                              new object[] { d, s, n, t }));

            var document = project.AddDocument(name, syntax);
            document = await ImplementInterfaces(workspace, document, new HashSet<string>(), generator, getCodeActions, cancellationToken);

            var rewriters = languageServices.GetLanguageServices<IDocumentRewriter>(project.Language).ToArray();
            foreach (var rewriter in rewriters)
            {
                document = await rewriter.VisitAsync(document, cancellationToken);
            }

            return document;
        }

        // Microsoft.CodeAnalysis.ImplementInterface.IImplementInterfaceService.GetCodeActions
        delegate IEnumerable<CodeAction> GetCodeActions(Document document, SemanticModel model, SyntaxNode node, CancellationToken cancellationToken);

        async Task<Document> ImplementInterfaces(Workspace workspace, Document document, HashSet<string> skipTypes, SyntaxGenerator generator, GetCodeActions getCodeActions, CancellationToken cancellationToken)
        {
            var project = document.Project;

            var compilation = await project.GetCompilationAsync(cancellationToken);
            var tree = await document.GetSyntaxTreeAsync(cancellationToken);
            var semantic = await document.GetSemanticModelAsync(cancellationToken);
            var syntax = tree.GetRoot().DescendantNodes().First(node => generator.GetDeclarationKind(node) == DeclarationKind.Class);
            var baseTypes = generator.GetBaseAndInterfaceTypes(syntax)
                .Where(x => !skipTypes.Contains(x.ToString()))
                .ToArray();

            if (baseTypes.Length == 0)
                return document;

            var actions = baseTypes.Select(baseType => new
            {
                BaseType = baseType,
                CodeActions = getCodeActions(document, semantic, baseType, cancellationToken)
#if DEBUG
                .ToArray()
#endif
            })
            .Where(x => x.CodeActions.Any())
#if DEBUG
            .ToArray()
#endif
            ;

            // We're done.
            if (!actions.Any())
                return document;

            var action = default(CodeAction);
            var current = actions.First();
            skipTypes.Add(current.BaseType.ToString());

            // The last base type is IProxy, we implement that explicitly always.
            // NOTE: VB doesn't have this concept, it always adds Implements [member]. 
            // So there is always a single CodeAction for it. The VisualBasicProxyRewriter takes 
            // care of making the property private, which is how you make it "explicit" in VB.
            if (current.BaseType == baseTypes.Last())
                action = current.CodeActions.Last();
            else
                action = current.CodeActions.First();

            // Otherwise, apply and recurse.
            try
            {
                var operations = await action.GetOperationsAsync(cancellationToken);
                var operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
                if (operation != null)
                {
                    operation.Apply(workspace, cancellationToken);
                    return await ImplementInterfaces(workspace, operation.ChangedSolution.GetDocument(document.Id), skipTypes, generator, getCodeActions, cancellationToken);
                }

                return document;
            }
            catch (OperationCanceledException)
            {
                throw new NotSupportedException($"Failed to apply code action '{action.Title}' for '{current.BaseType.ToString()}'. Document: \r\n{syntax.NormalizeWhitespace().ToString()}");
            }
        }

        class AttributeFilterProvider : AttributedModelProvider
        {
            public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, MemberInfo member) =>
                member.GetCustomAttributes().Where(x => !(x is ExtensionOrderAttribute));

            public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, ParameterInfo member) =>
                member.GetCustomAttributes().Where(x => !(x is ExtensionOrderAttribute));
        }
    }
}