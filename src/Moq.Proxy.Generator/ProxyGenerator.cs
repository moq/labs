using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Convention;
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

                /*
                // These two groups are already part of MefHostServices.DefaultAssemblies
                Assembly.Load("Microsoft.CodeAnalysis.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.VisualBasic.Workspaces"),

                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.VisualBasic.Features"),
                */
            })
            .ToImmutableArray();

        static ImmutableArray<Assembly> DefaultAssemblies => generatorDefaultAssemblies;

        public static HostServices CreateHost() => MefHostServices.Create(DefaultAssemblies);

        public async Task<Document> GenerateProxyAsync(Workspace workspace, Project project, 
            CancellationToken cancellationToken, params INamedTypeSymbol[] types)
        {
            // TODO: the project *must* have a reference to the Moq.Proxy assembly. How do we verify that?

            // Sort interfaces so regardless of order, we reuse the proxies
            if (types.FirstOrDefault()?.TypeKind == TypeKind.Interface)
                Array.Sort(types, Comparer<INamedTypeSymbol>.Create((x, y) => x.Name.CompareTo(y.Name)));
            else if (types.Length > 1)
                Array.Sort(types, 1, types.Length - 1, Comparer<INamedTypeSymbol>.Create((x, y) => x.Name.CompareTo(y.Name)));

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
            document = await ImplementInterfaces(workspace, document, generator, getCodeActions, cancellationToken);

            var rewriters = languageServices.GetLanguageServices<IDocumentRewriter>(project.Language).ToArray();
            foreach (var rewriter in rewriters)
            {
                document = await rewriter.VisitAsync(document, cancellationToken);
            }

            return document;
        }

        // Microsoft.CodeAnalysis.ImplementInterface.IImplementInterfaceService.GetCodeActions
        delegate IEnumerable<CodeAction> GetCodeActions(Document document, SemanticModel model, SyntaxNode node, CancellationToken cancellationToken);

        async Task<Document> ImplementInterfaces(Workspace workspace, Document document, SyntaxGenerator generator, GetCodeActions getCodeActions, CancellationToken cancellationToken)
        {
            var project = document.Project;

            var compilation = await project.GetCompilationAsync(cancellationToken);
            var tree = await document.GetSyntaxTreeAsync(cancellationToken);
            var semantic = await document.GetSemanticModelAsync(cancellationToken);
            var syntax = tree.GetRoot().DescendantNodes().First(node => generator.GetDeclarationKind(node) == DeclarationKind.Class);
            var baseTypes = generator.GetBaseAndInterfaceTypes(syntax);

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
            // The last base type is IProxy, we implement that explicitly always.
            // NOTE: VB doesn't have this concept, it always adds Implements [member]. 
            // So there is always a single CodeAction for it. The VisualBasicProxyRewriter takes 
            // care of making the property private, which is how you make it "explicit" in VB.
            if (current.BaseType == baseTypes.Last())
                action = current.CodeActions.Last();
            else
                action = current.CodeActions.First();
            
            // Otherwise, apply and recurse.
            var operations = await action.GetOperationsAsync(cancellationToken);
            var operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
            if (operation != null)
            {
                operation.Apply(workspace, cancellationToken);
                return await ImplementInterfaces(workspace, operation.ChangedSolution.GetDocument(document.Id), generator, getCodeActions, cancellationToken);
            }

            return document;
        }

        class AttributeFilterProvider : AttributedModelProvider
        {
            public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, MemberInfo member)
            {
                var customAttributes = member.GetCustomAttributes().Where(x => !(x is ExtensionOrderAttribute)).ToArray();
                return customAttributes;
            }

            public override IEnumerable<Attribute> GetCustomAttributes(Type reflectedType, ParameterInfo member)
            {
                var customAttributes = member.GetCustomAttributes().Where(x => !(x is ExtensionOrderAttribute)).ToArray();
                return customAttributes;
            }
        }
    }
}