using System.Linq;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy
{
    static class ICodeAnalysisServicesExtensions
    {
        public static ILanguageService GetLanguageService(this ICodeAnalysisServices services, string language, string serviceType, string layer = ServiceLayer.Default)
            => services.GetLanguageServices(language, serviceType, layer).FirstOrDefault();

        public static TService GetLanguageService<TService>(this ICodeAnalysisServices services, string language, string layer = ServiceLayer.Default)
            where TService : ILanguageService
            => services.GetLanguageServices<TService>(language, layer).FirstOrDefault();
    }
}