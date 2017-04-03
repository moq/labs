using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Simplification;

namespace Moq.Proxy
{
    public class ProxyGenerator
    {
        // Used for MEF composition.
        static readonly ImmutableArray<Assembly> generatorDefaultAssemblies = MefHostServices.DefaultAssemblies
            .Concat(new[]
            {
                Assembly.GetExecutingAssembly(),
                Assembly.Load("Microsoft.CodeAnalysis"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp"),
                Assembly.Load("Microsoft.CodeAnalysis.VisualBasic"),
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.VisualBasic.Features"),
            })
            .ToImmutableArray();

        // Built from the above assemblies.
        static readonly CompositionHost generatorComposition = new ContainerConfiguration()
                .WithAssemblies(generatorDefaultAssemblies)
                .CreateContainer();

        static ImmutableArray<Assembly> DefaultAssemblies => generatorDefaultAssemblies;

        public static CompositionHost DefaultComposition => generatorComposition;

        public static HostServices DefaultHost => MefHostServices.Create(DefaultAssemblies);
        
        public async Task<Document> GenerateProxyAsync(Workspace workspace, Project project, params INamedTypeSymbol[] types)
        {
            // TODO: the project *must* have a reference to the Moq.Proxy assembly. How do we verify that?

            // Sort interfaces so regardless of order, we reuse the proxies
            if (types.FirstOrDefault()?.TypeKind == TypeKind.Interface)
                Array.Sort<INamedTypeSymbol>(types, Comparer<INamedTypeSymbol>.Create((x, y) => x.Name.CompareTo(y.Name)));
            else if (types.Length > 1)
                Array.Sort<INamedTypeSymbol>(types, 1, types.Length - 1, Comparer<INamedTypeSymbol>.Create((x, y) => x.Name.CompareTo(y.Name)));

            var generator = SyntaxGenerator.GetGenerator(project);
            var name = "ProxyOf" + string.Join("", types.Select(x => x.Name));

            var syntax = generator.CompilationUnit(types
                .Where(x => x.ContainingNamespace != null)
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

            var languageServices = DefaultComposition.GetExport<LanguageServiceRetriever>();

            var implementInterfaceService = languageServices.GetLanguageServices(project.Language,
                "Microsoft.CodeAnalysis.ImplementInterface.IImplementInterfaceService").FirstOrDefault();

            var getCodeActionsMethod = implementInterfaceService.GetType().GetMethod("GetCodeActions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            GetCodeActions getCodeActions = (d, s, n, t) => ((IEnumerable<CodeAction>)getCodeActionsMethod.Invoke(
                              implementInterfaceService,
                              new object[] { d, s, n, t }));

            var document = project.AddDocument(name, syntax.NormalizeWhitespace().ToFullString());
            document = await ImplementInterfaces(workspace, document, generator, getCodeActions);

            var rewriters = languageServices.GetLanguageServices<IDocumentRewriter>(project.Language).ToArray();
            foreach (var rewriter in rewriters)
            {
                document = await rewriter.VisitAsync(document, CancellationToken.None);
            }

            return await Simplifier.ReduceAsync(document);
        }

        // Microsoft.CodeAnalysis.ImplementInterface.IImplementInterfaceService.GetCodeActions
        delegate IEnumerable<CodeAction> GetCodeActions(Document document, SemanticModel model, SyntaxNode node, CancellationToken cancellationToken);

        async Task<Document> ImplementInterfaces(Workspace workspace, Document document, SyntaxGenerator generator, GetCodeActions getCodeActions)
        {
            var project = document.Project;

            var compilation = await project.GetCompilationAsync();
            var tree = await document.GetSyntaxTreeAsync();
            var semantic = await document.GetSemanticModelAsync();
            var syntax = tree.GetRoot().DescendantNodes().First(node => generator.GetDeclarationKind(node) == DeclarationKind.Class);
            var baseTypes = generator.GetBaseAndInterfaceTypes(syntax);

            var actions = baseTypes.Select(baseType => new
            {
                BaseType = baseType,
                CodeActions = getCodeActions(document, semantic, baseType, CancellationToken.None)
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
            // The last base type is IProxy, we implement that explicitly always.
            // NOTE: VB doesn't have this concept, it always adds Implements [member]. 
            // So there is always a single CodeAction for it. The VisualBasicProxyRewriter takes 
            // care of making the property private, which is how you make it "explicit" in VB.
            if (current.BaseType == baseTypes.Last())
                action = current.CodeActions.Last();
            else
                action = current.CodeActions.First();

            // Otherwise, apply and recurse.
            var operations = await action.GetOperationsAsync(CancellationToken.None);
            var operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
            if (operation != null)
            {
                operation.Apply(workspace, CancellationToken.None);
                return await ImplementInterfaces(workspace, operation.ChangedSolution.GetDocument(document.Id), generator, getCodeActions);
            }

            return document;
        }
    }
}
