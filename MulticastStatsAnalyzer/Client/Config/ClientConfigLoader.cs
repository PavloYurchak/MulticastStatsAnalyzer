using Shared.Config;
using System.Xml.Linq;

namespace Client.Config
{
    public static class ClientConfigLoader
    {
        public static ClientConfig Load(string path)
        {
            var doc = XDocument.Load(path);
            var root = doc.Element("ClientConfig")
                       ?? throw new InvalidOperationException("Root element <ClientConfig> not found");

            return new ClientConfig
            {
                MulticastAddress = ConfigUtils.GetString(root, "MulticastAddress"),
                Port = ConfigUtils.GetInt(root, "Port"),
                BufferSize = ConfigUtils.GetInt(root, "BufferSize"),
                ReceiveTimeoutMs = ConfigUtils.GetInt(root, "ReceiveTimeoutMs")
            };
        }
    }
}
