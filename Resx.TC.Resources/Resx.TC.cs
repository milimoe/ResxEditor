using System.Resources;
using System.Globalization;

namespace ResxEditor
{
    public class ResourceTC
    {
        public static ResourceManager ResourceManager = new("ResxEditor.ResourceTC.Resx.TC", typeof(ResourceTC).Assembly);

        public static string GetString(string key)
        {
            string? str = ResourceManager.GetString(key, CultureInfo.InvariantCulture);
            if (str != null) return str;
            return "";
        }
    }
}