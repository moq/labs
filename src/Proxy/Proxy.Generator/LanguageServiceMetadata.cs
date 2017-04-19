using System.Collections.Generic;

namespace Moq.Proxy
{
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