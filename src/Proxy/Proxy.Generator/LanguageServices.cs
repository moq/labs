using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy
{
    [ExportWorkspaceService(typeof(ILanguageServices))]
    [Shared]
    class LanguageServices : IWorkspaceService, ILanguageServices
    {
        IEnumerable<Lazy<ILanguageService, LanguageServiceMetadata>> services;

        [ImportingConstructor]
        public LanguageServices([ImportMany] IEnumerable<Lazy<ILanguageService, LanguageServiceMetadata>> services)
            => this.services = services ?? throw new ArgumentNullException(nameof(services));

        public IEnumerable<ILanguageService> GetLanguageServices(string language, string serviceType, string layer = ServiceLayer.Default)
            => services
                .Where(x => 
                    x.Metadata.Language == language && 
                    x.Metadata.Layer == layer &&
                    x.Metadata.ServiceType.StartsWith(serviceType))
                .Select(x => x.Value);

        public IEnumerable<TService> GetLanguageServices<TService>(string language, string layer = ServiceLayer.Default)
            => services
                .Where(x => 
                    x.Metadata.Language == language && 
                    x.Metadata.Layer == layer &&
                    x.Metadata.ServiceType.StartsWith(typeof(TService).FullName))
                .Select(x => x.Value)
                .OfType<TService>();
    }
}
