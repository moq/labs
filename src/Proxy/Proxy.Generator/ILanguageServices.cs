using System.Collections.Generic;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy
{
    interface ILanguageServices : IWorkspaceService
    {
        IEnumerable<ILanguageService> GetLanguageServices(string language, string serviceType, string layer = ServiceLayer.Default);

        IEnumerable<TService> GetLanguageServices<TService>(string language, string layer = ServiceLayer.Default);
    }
}