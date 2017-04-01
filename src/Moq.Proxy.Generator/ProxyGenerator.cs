using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        static readonly ImmutableArray<Type> defaultReferenceAssemblyTypes = new[]
        {
            typeof(object),
            typeof(Thread),
            typeof(Task),
            typeof(List<>),
            typeof(Regex),
            typeof(StringBuilder),
            typeof(Uri),
            typeof(Enumerable),
            typeof(IEnumerable),
            typeof(Path),
            typeof(Assembly),
        }.ToImmutableArray();

        static readonly ImmutableArray<Assembly> defaultReferenceAssemblies = defaultReferenceAssemblyTypes
            .Select(x => x.Assembly).Distinct().Concat(new[]
            {
                Assembly.Load("System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly,
            })
            .ToImmutableArray();

        static readonly ImmutableArray<Assembly> generatorDefaultAssemblies = MefHostServices.DefaultAssemblies
            .Concat(new[]
            {
                Assembly.Load("Microsoft.CodeAnalysis"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp"),
                Assembly.Load("Microsoft.CodeAnalysis.VisualBasic"),
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.VisualBasic.Features"),
                //Assembly.Load("Microsoft.CodeAnalysis.EditorFeatures")
            })
            .ToImmutableArray();

        static readonly CompositionHost generatorComposition = new ContainerConfiguration()
                .WithAssemblies(generatorDefaultAssemblies)
                .CreateContainer();

        static ImmutableArray<Assembly> DefaultAssemblies => generatorDefaultAssemblies;

        public static CompositionHost DefaultComposition => generatorComposition;

        public static HostServices DefaultHost => MefHostServices.Create(DefaultAssemblies);
        
        public async Task<Document> GenerateProxyAsync(Workspace workspace, Project project, params INamedTypeSymbol[] types)
        {
            var generator = project.LanguageServices?.GetService<SyntaxGenerator>();
            var name = "ProxyOf" + string.Join("", types.Select(x => x.Name));

            var syntax = generator.CompilationUnit(types
                .Where(x => x.ContainingNamespace != null)
                .Select(x => x.ContainingNamespace.ToDisplayString())
                .Distinct()
                .Select(x => generator.NamespaceImportDeclaration(x))
                .Concat(new[]
                {
                    generator.ClassDeclaration(name,
                        accessibility: Accessibility.Public,
                        interfaceTypes: types.Select(x => generator.IdentifierName(x.Name)))
                }));
                
            var languageServices = (IEnumerable<Lazy<ILanguageService, IDictionary<string, object>>>)
                DefaultComposition.GetExports(typeof(Lazy<ILanguageService, IDictionary<string, object>>));

            var implementInterfaceService = languageServices.FirstOrDefault(x =>
                (string)x.Metadata[nameof(ExportLanguageServiceAttribute.Language)] == project.Language &&
                x.Metadata[nameof(ExportLanguageServiceAttribute.ServiceType)].ToString().StartsWith("Microsoft.CodeAnalysis.ImplementInterface.IImplementInterfaceService"))
                ?.Value;

            var getCodeActionsMethod = implementInterfaceService.GetType().GetMethod("GetCodeActions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            GetCodeActions getCodeActions = (d, s, n, t) => ((IEnumerable<CodeAction>)getCodeActionsMethod.Invoke(
                              implementInterfaceService,
                              new object[] { d, s, n, t }));

            var document = project.AddDocument(name, syntax.NormalizeWhitespace().ToFullString());
            document = await ImplementInterfaces(workspace, document, generator, getCodeActions);

            // Annotate the base interfaces for reduction.
            var tree = await document.GetSyntaxTreeAsync();
            syntax = tree.GetRoot().DescendantNodes().First(node => generator.GetDeclarationKind(node) == DeclarationKind.Class);
            var baseTypes = generator.GetBaseAndInterfaceTypes(syntax);

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
                        
            var action = baseTypes.Select(baseType => getCodeActions(document, semantic, baseType, CancellationToken.None).FirstOrDefault())
                .Where(x => x != null)
                .FirstOrDefault();

            // We're done.
            if (action == null)
                return document;

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
