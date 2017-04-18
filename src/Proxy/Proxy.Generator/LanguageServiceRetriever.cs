using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy
{
    [ExportWorkspaceService(typeof(LanguageServiceRetriever))]
    class LanguageServiceRetriever : IWorkspaceService
    {
        IEnumerable<Lazy<ILanguageService, LanguageServiceMetadata>> services;

        [ImportingConstructor]
        public LanguageServiceRetriever([ImportMany] IEnumerable<Lazy<ILanguageService, LanguageServiceMetadata>> services)
            => this.services = services ?? throw new ArgumentNullException(nameof(services));

        public IEnumerable<ILanguageService> GetLanguageServices(string language, string serviceType)
            => services
                .Where(x => x.Metadata.Language == language && x.Metadata.ServiceType.StartsWith(serviceType))
                .Select(x => x.Value);

        public IEnumerable<TService> GetLanguageServices<TService>(string language)
            => services
                .Where(x => x.Metadata.Language == language && x.Metadata.ServiceType.StartsWith(typeof(TService).FullName))
                .Select(x => x.Value)
                .OfType<TService>();

        public class LanguageServiceMetadata
        {
            public string Language { get; }

            public string ServiceType { get; }

            public IDictionary<string, object> Data { get; }

            public LanguageServiceMetadata(IDictionary<string, object> data)
            {
                Language = (string)(data.TryGetValue(nameof(Language), out var language) ? language : default(string));
                ServiceType = (string)(data.TryGetValue(nameof(ServiceType), out var service) ? service : default(string));
                Data = data;
            }
        }
    }
}
