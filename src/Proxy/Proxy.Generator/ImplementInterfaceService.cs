using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Host;
using Moq.Proxy.Properties;

namespace Moq.Proxy
{
    class ImplementInterfaceService
    {
        ILanguageService languageService;
        MethodBase getCodeActions;

        public ImplementInterfaceService(LanguageServiceRetriever languageServices, string languageName)
        {
            languageService = languageServices
                .GetLanguageService(languageName, "Microsoft.CodeAnalysis.ImplementInterface.IImplementInterfaceService")
                ?? throw new ArgumentException(Strings.UnsupportedLanguage("IImplementInterfaceService", languageName));

            getCodeActions = languageService.GetType().GetMethod(
                nameof(GetCodeActions), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public IEnumerable<CodeAction> GetCodeActions(Document document, SemanticModel model, SyntaxNode node, CancellationToken cancellationToken) => 
            ((IEnumerable<CodeAction>)getCodeActions.Invoke(
                languageService, new object[] { document, model, node, cancellationToken }));
    }
}