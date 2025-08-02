using Shared.Config;
using System.Xml.Linq;

namespace Server.Config
{
    public static class ConfigLoader
    {
        public static ServerConfig Load(string path)
        {
            var doc = XDocument.Load(path);
            var root = doc.Element("ServerConfig")
                   ?? throw new InvalidOperationException("Root element <ServerConfig> not found");

            return new ServerConfig
            {
                MulticastAddress = ConfigUtils.GetString(root, "MulticastAddress"),
                Port = ConfigUtils.GetInt(root, "Port"),
                MinValue = ConfigUtils.GetDouble(root, "MinValue"),
                MaxValue = ConfigUtils.GetDouble(root, "MaxValue"),
                Decimals = ConfigUtils.GetInt(root, "Decimals")
            };
        }
    }
}
