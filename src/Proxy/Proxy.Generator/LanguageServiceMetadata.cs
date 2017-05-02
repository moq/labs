using System.Collections.Generic;

namespace Moq.Proxy
{
    /// <summary>
    /// Metadata annotations for <see cref="Microsoft.CodeAnalysis.Host.ILanguageService"/> services.
    /// </summary>
    public class LanguageServiceMetadata
    {
        /// <summary>
        /// The target language for the service.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The layer of the provided service.
        /// </summary>
        public string Layer { get; set; }

        /// <summary>
        /// The service implementation type.
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// Original service registration metadata.
        /// </summary>
        public IDictionary<string, object> Data { get; }

        /// <summary>
        /// Instantiates an empty <see cref="LanguageServiceMetadata"/>.
        /// </summary>
        public LanguageServiceMetadata() { }

        /// <summary>
        /// Instantiates the <see cref="LanguageServiceMetadata"/> from the 
        /// given metadata values.
        /// </summary>
        public LanguageServiceMetadata(IDictionary<string, object> data)
        {
            Language = (string)(data.TryGetValue(nameof(Language), out var language) ? language : default(string));
            Layer = (string)(data.TryGetValue(nameof(Layer), out var layer) ? layer : default(string));
            ServiceType = (string)(data.TryGetValue(nameof(ServiceType), out var service) ? service : default(string));
            Data = data;
        }

        /// <summary>
        /// String representation of the language service metadata.
        /// </summary>
        public override string ToString()
        {
            return $"{ServiceType} ({Language} @ {Layer})";
        }
    }
}