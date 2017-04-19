using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host;
using Moq.Proxy.Properties;

namespace Moq.Proxy
{
    class ImplementAbstractClassService
    {
        ILanguageService languageService;
        CanImplementAbstractClassAsync canImplement;
        ImplementAbstractClassAsync implement;

        public ImplementAbstractClassService(LanguageServiceRetriever languageServices, string languageName)
        {
            languageService = languageServices
                .GetLanguageService(languageName, "Microsoft.CodeAnalysis.ImplementAbstractClass.IImplementAbstractClassService")
                ?? throw new ArgumentException(Strings.UnsupportedLanguage("IImplementAbstractClassService", languageName));

            var canImplementMethod = languageService.GetType().GetMethod(nameof(CanImplementAbstractClassAsync), 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod) 
                ?? throw new ArgumentException(Strings.ServiceMethodNotFound(nameof(CanImplementAbstractClassAsync)), nameof(languageService));

            canImplement = (document, classNode, cancellationToken) =>
                (Task<bool>)canImplementMethod.Invoke(languageService, new object[] { document, classNode, cancellationToken });

            var implementMethod = languageService.GetType().GetMethod(nameof(ImplementAbstractClassAsync),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod)
                ?? throw new ArgumentException(Strings.ServiceMethodNotFound(nameof(ImplementAbstractClassAsync)), nameof(languageService));

            implement = (document, classNode, cancellationToken) =>
                (Task<Document>)implementMethod.Invoke(languageService, new object[] { document, classNode, cancellationToken });
        }

        public async Task<Document> ImplementAbstractClass(Document document, CancellationToken cancellationToken)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            var project = document.Project;
            var compilation = await project.GetCompilationAsync(cancellationToken);

            var tree = await document.GetSyntaxTreeAsync(cancellationToken);
            var semantic = await document.GetSemanticModelAsync(cancellationToken);
            var syntax = tree.GetRoot().DescendantNodes().FirstOrDefault(node => generator.GetDeclarationKind(node) == DeclarationKind.Class);

            if (syntax == null)
                return document;

            if (await canImplement(document, syntax, cancellationToken))
                return await implement(document, syntax, cancellationToken);

            return document;
        }

        delegate Task<bool> CanImplementAbstractClassAsync(Document document, SyntaxNode classNode, CancellationToken cancellationToken);
        delegate Task<Document> ImplementAbstractClassAsync(Document document, SyntaxNode classNode, CancellationToken cancellationToken);
    }
}