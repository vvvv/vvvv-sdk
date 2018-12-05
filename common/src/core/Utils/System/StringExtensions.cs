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

        /// <summary>
        /// Converts the first letter of a string to upper case, using the casing rules of invariant culture.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToUpperFirstInvariant(this string s)
        {
            // Check for empty string.
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            if (s.Length > 1)
            {
                // Return char and concat substring.
                return char.ToUpperInvariant(s[0]) + s.Substring(1);
            }
            else
            {
                return s.ToUpperInvariant();
            }
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source != null && source.IndexOf(value, comparisonType) >= 0;
        }

        public static string ToEnvironmentNewLine(this string value)
        {
            if (Environment.NewLine != "\n")
                return value.Replace("\n", Environment.NewLine);
            return value;
        }
    }
}
