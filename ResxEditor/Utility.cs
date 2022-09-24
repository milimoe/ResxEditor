using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxEditor
{
    public class Utility
    {
        public static string GetString(string key, int language = -1)
        {
            if (language == -1) language = ResxEditor.NowLanguage;
            return language switch
            {
                0 => ResourceEng.GetString(key),
                1 => ResourceSC.GetString(key),
                2 => ResourceTC.GetString(key),
                _ => ""
            };
        }
    }
}
