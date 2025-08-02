using System.Globalization;
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
                MulticastAddress = GetString(root, "MulticastAddress"),
                Port = GetInt(root, "Port"),
                MinValue = GetDouble(root, "MinValue"),
                MaxValue = GetDouble(root, "MaxValue"),
                Decimals = GetInt(root, "Decimals")
            };
        }

        private static string GetString(XElement root, string name)
        {
            var el = root.Element(name)
                ?? throw new InvalidOperationException($"Missing required config element: <{name}>");
            return el.Value;
        }

        private static int GetInt(XElement root, string name)
        {
            return int.Parse(GetString(root, name), CultureInfo.InvariantCulture);
        }

        private static double GetDouble(XElement root, string name)
        {
            return double.Parse(GetString(root, name), CultureInfo.InvariantCulture);
        }

    }
}
