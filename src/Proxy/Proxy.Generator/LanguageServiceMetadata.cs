using System.Collections.Generic;

namespace Moq.Proxy
{
    public class LanguageServiceMetadata
    {
        public string Language { get; set; }

        public string Layer { get; set; }

        public string ServiceType { get; set; }

        public IDictionary<string, object> Data { get; }

        public LanguageServiceMetadata() { }

        public LanguageServiceMetadata(IDictionary<string, object> data)
        {
            Language = (string)(data.TryGetValue(nameof(Language), out var language) ? language : default(string));
            Layer = (string)(data.TryGetValue(nameof(Layer), out var layer) ? layer : default(string));
            ServiceType = (string)(data.TryGetValue(nameof(ServiceType), out var service) ? service : default(string));
            Data = data;
        }

        public override string ToString()
        {
            return $"{ServiceType} ({Language})"; 
        }
    }
}