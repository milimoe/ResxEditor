using System.Resources;
using System.Globalization;

namespace ResxEditor
{
    public class ResourceEng
    {
        public static ResourceManager ResourceManager = new("ResxEditor.ResourceEng.Resx.Eng", typeof(ResourceEng).Assembly);

        public static string GetString(string key)
        {
            string? str = ResourceManager.GetString(key, CultureInfo.InvariantCulture);
            if (str != null) return str;
            return "";
        }
    }
}
