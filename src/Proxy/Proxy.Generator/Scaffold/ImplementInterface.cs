using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using Moq.Proxy.Properties;

namespace Moq.Proxy.Scaffold
{
    abstract class ImplementInterface : IDocumentVisitor
    {
        string language;
        GetCodeActions getCodeActions;
        ICodeAnalysisServices services;

        protected ImplementInterface(ICodeAnalysisServices services, string language)
        {
            this.services = services;
            this.language = language;
        }

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            // The export is cached/shared, so we can safely cache the MEF language service too.
            if (getCodeActions == null)
                Initialize(services);

            return await ImplementInterfaces(document, new HashSet<string>(), SyntaxGenerator.GetGenerator(document), cancellationToken);
        }

        async Task<Document> ImplementInterfaces(Document document, HashSet<string> skipTypes, SyntaxGenerator generator, CancellationToken cancellationToken)
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
                    operation.Apply(project.LanguageServices.WorkspaceServices.Workspace, cancellationToken);
                    return await ImplementInterfaces(operation.ChangedSolution.GetDocument(document.Id), skipTypes, generator, cancellationToken);
                }

                return document;
            }
            catch (OperationCanceledException)
            {
                throw new NotSupportedException($"Failed to apply code action '{action.Title}' for '{current.BaseType.ToString()}'. Document: \r\n{syntax.NormalizeWhitespace().ToString()}");
            }
        }

        void Initialize(ICodeAnalysisServices services)
        {
            var service = services
                .GetLanguageService(language, "Microsoft.CodeAnalysis.ImplementInterface.IImplementInterfaceService")
                ?? throw new ArgumentException(Strings.UnsupportedLanguage("IImplementInterfaceService", language));

            var getCodeActionsMethod = service.GetType().GetMethod(
                nameof(GetCodeActions), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

            getCodeActions = (document, semantic, node, cancellationToken)
                => ((IEnumerable<CodeAction>)getCodeActionsMethod.Invoke(
                    service, new object[] { document, semantic, node, cancellationToken }));
        }

        // Microsoft.CodeAnalysis.ImplementInterface.IImplementInterfaceService.GetCodeActions
        delegate IEnumerable<CodeAction> GetCodeActions(Document document, SemanticModel model, SyntaxNode node, CancellationToken cancellationToken);
    }
}