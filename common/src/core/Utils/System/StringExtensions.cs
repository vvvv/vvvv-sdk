using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string s)
        {
            switch (s.Length)
            {
                case 0:
                    return s;
                case 1:
                    return s.ToLower();
                default:
                    var titleCase = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s);
                    return titleCase.Substring(0, 1).ToLower() + titleCase.Substring(1);
            }
        }
    }
}
