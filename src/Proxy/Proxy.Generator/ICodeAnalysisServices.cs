using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy
{
    interface ICodeAnalysisServices : IWorkspaceService
    {
        IEnumerable<ILanguageService> GetLanguageServices(string language, string serviceType, string layer = ServiceLayer.Default);

        IEnumerable<TService> GetLanguageServices<TService>(string language, string layer = ServiceLayer.Default)
            where TService : ILanguageService;

        IWorkspaceService GetWorkspaceService(string serviceType, string layer = ServiceLayer.Default);

        TService GetWorkspaceService<TService>(string layer = ServiceLayer.Default)
            where TService : class, IWorkspaceService;
    }
}