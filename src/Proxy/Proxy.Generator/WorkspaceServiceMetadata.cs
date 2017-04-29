using System.Collections.Generic;

namespace Moq.Proxy
{
    public class WorkspaceServiceMetadata
    {
        public string Layer { get; set; }

        public string ServiceType { get; set; }

        public IDictionary<string, object> Data { get; }

        public WorkspaceServiceMetadata() { }

        public WorkspaceServiceMetadata(IDictionary<string, object> data)
        {
            Layer = (string)(data.TryGetValue(nameof(Layer), out var layer) ? layer : default(string));
            ServiceType = (string)(data.TryGetValue(nameof(ServiceType), out var service) ? service : default(string));
            Data = data;
        }

        public override string ToString()
        {
            return $"{ServiceType} ({Layer})"; 
        }
    }
}