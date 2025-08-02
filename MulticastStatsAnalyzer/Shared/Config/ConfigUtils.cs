using System.Globalization;
using System.Xml.Linq;

namespace Shared.Config
{
    public static class ConfigUtils
    {
        public static string GetString(XElement root, string name)
        {
            var el = root.Element(name)
                ?? throw new InvalidOperationException($"Missing required config element: <{name}>");
            return el.Value;
        }

        public static int GetInt(XElement root, string name)
        {
            return int.Parse(GetString(root, name), CultureInfo.InvariantCulture);
        }

        public static double GetDouble(XElement root, string name)
        {
            return double.Parse(GetString(root, name), CultureInfo.InvariantCulture);
        }
    }
}
