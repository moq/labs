using System.Collections.Generic;

namespace Moq.Proxy
{
    /// <summary>
    /// Metadata annotations for <see cref="Microsoft.CodeAnalysis.Host.IWorkspaceService"/> services.
    /// </summary>
    public class WorkspaceServiceMetadata
    {
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
        /// Instantiates an empty <see cref="WorkspaceServiceMetadata"/>.
        /// </summary>
        public WorkspaceServiceMetadata() { }

        /// <summary>
        /// Instantiates the <see cref="WorkspaceServiceMetadata"/> from the 
        /// given metadata values.
        /// </summary>
        public WorkspaceServiceMetadata(IDictionary<string, object> data)
        {
            Layer = (string)(data.TryGetValue(nameof(Layer), out var layer) ? layer : default(string));
            ServiceType = (string)(data.TryGetValue(nameof(ServiceType), out var service) ? service : default(string));
            Data = data;
        }

        /// <summary>
        /// String representation of the workspace service metadata.
        /// </summary>
        public override string ToString()
        {
            return $"{ServiceType} ({Layer})";
        }
    }
}