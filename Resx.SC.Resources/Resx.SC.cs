using System.Resources;
using System.Globalization;

namespace ResxEditor
{
    public class ResourceSC
    {
        public static ResourceManager ResourceManager = new("ResxEditor.ResourceSC.Resx.SC", typeof(ResourceSC).Assembly);

        public static string GetString(string key)
        {
            string? str = ResourceManager.GetString(key, CultureInfo.InvariantCulture);
            if (str != null) return str;
            return "";
        }
    }
}