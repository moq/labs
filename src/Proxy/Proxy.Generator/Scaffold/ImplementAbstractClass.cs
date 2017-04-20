using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host;
using Moq.Proxy.Properties;

namespace Moq.Proxy.Scaffold
{
    abstract class ImplementAbstractClass : IDocumentVisitor
    {
        string language;

        ILanguageService languageService;
        CanImplementAbstractClassAsync canImplement;
        ImplementAbstractClassAsync implement;

        protected ImplementAbstractClass(string language) => this.language = language;

        public async Task<Document> VisitAsync(ILanguageServices services, Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            // The export is cached/shared, so we can safely cache the MEF language service too.
            if (languageService == null)
                Initialize(services);

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

        void Initialize(ILanguageServices services)
        {
            languageService = services
                .GetLanguageService(language, "Microsoft.CodeAnalysis.ImplementAbstractClass.IImplementAbstractClassService")
                ?? throw new ArgumentException(Strings.UnsupportedLanguage("IImplementAbstractClassService", language));

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

        delegate Task<bool> CanImplementAbstractClassAsync(Document document, SyntaxNode classNode, CancellationToken cancellationToken);
        delegate Task<Document> ImplementAbstractClassAsync(Document document, SyntaxNode classNode, CancellationToken cancellationToken);
    }
}