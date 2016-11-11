using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VVVV.Utils
{

    /// <summary>
    /// Specifies ellipsis format and alignment.
    /// </summary>
    [Flags]
    public enum EllipsisFormat
    {
        /// <summary>
        /// Text is not modified.
        /// </summary>
        None = 0,
        /// <summary>
        /// Text is trimmed at the end of the string. An ellipsis (...) is drawn in place of remaining text.
        /// </summary>
        End = 1,
        /// <summary>
        /// Text is trimmed at the begining of the string. An ellipsis (...) is drawn in place of remaining text. 
        /// </summary>
        Start = 2,
        /// <summary>
        /// Text is trimmed in the middle of the string. An ellipsis (...) is drawn in place of remaining text.
        /// </summary>
        Middle = 3,
        /// <summary>
        /// Preserve as much as possible of the drive and filename information. Must be combined with alignment information.
        /// </summary>
        Path = 4,
        /// <summary>
        /// Text is trimmed at a word boundary. Must be combined with alignment information.
        /// </summary>
        Word = 8
    }


    public class Ellipsis
    {
        /// <summary>
        /// String used as a place holder for trimmed text.
        /// </summary>
        public static readonly string EllipsisChars = "...";

        private static Regex prevWord = new Regex(@"\W*\w*$");
        private static Regex nextWord = new Regex(@"\w*\W*");

        /// <summary>
        /// Truncates a text string to fit within a given control width by replacing trimmed text with ellipses. 
        /// </summary>
        /// <param name="text">String to be trimmed.</param>
        /// <param name="width">text must fit within width.</param>
        ///	<param name="font">font is used to measure the text string.</param>
        /// <param name="options">Format and alignment of ellipsis.</param>
        /// <returns>This function returns text trimmed to the specified witdh.</returns>
        public static string Compact(string text, int width, Font font, EllipsisFormat options)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // no aligment information
            if ((EllipsisFormat.Middle & options) == 0)
                return text;

            if (font == null)
                throw new ArgumentNullException("font");

            {
                Size s = TextRenderer.MeasureText(text, font);

                // control is large enough to display the whole text
                if (s.Width <= width)
                    return text;

                string pre = "";
                string mid = text;
                string post = "";

                bool isPath = (EllipsisFormat.Path & options) != 0;

                // split path string into <drive><directory><filename>
                if (isPath)
                {
                    pre = Path.GetPathRoot(text);
                    mid = Path.GetDirectoryName(text).Substring(pre.Length);
                    post = Path.GetFileName(text);
                }

                int len = 0;
                int seg = mid.Length;
                string fit = "";

                // find the longest string that fits into 
                // the control boundaries using bisection method
                while (seg > 1)
                {
                    seg -= seg / 2;

                    int left = len + seg;
                    int right = mid.Length;

                    if (left > right)
                        continue;

                    if ((EllipsisFormat.Middle & options) == EllipsisFormat.Middle)
                    {
                        right -= left / 2;
                        left -= left / 2;
                    }
                    else if ((EllipsisFormat.Start & options) != 0)
                    {
                        right -= left;
                        left = 0;
                    }

                    // trim at a word boundary using regular expressions
                    if ((EllipsisFormat.Word & options) != 0)
                    {
                        if ((EllipsisFormat.End & options) != 0)
                        {
                            left -= prevWord.Match(mid, 0, left).Length;
                        }
                        if ((EllipsisFormat.Start & options) != 0)
                        {
                            right += nextWord.Match(mid, right).Length;
                        }
                    }

                    // build and measure a candidate string with ellipsis
                    string tst = mid.Substring(0, left) + EllipsisChars + mid.Substring(right);

                    // restore path with <drive> and <filename>
                    if (isPath)
                    {
                        tst = Path.Combine(Path.Combine(pre, tst), post);
                    }
                    s = TextRenderer.MeasureText(tst, font);

                    // candidate string fits into control boundaries, try a longer string
                    // stop when seg <= 1
                    if (s.Width <= width)
                    {
                        len += seg;
                        fit = tst;
                    }
                }

                if (len == 0) // string can't fit into control
                {
                    // "path" mode is off, just return ellipsis characters
                    if (!isPath)
                        return EllipsisChars;

                    // <drive> and <directory> are empty, return <filename>
                    if (pre.Length == 0 && mid.Length == 0)
                        return post;

                    // measure "C:\...\filename.ext"
                    fit = Path.Combine(Path.Combine(pre, EllipsisChars), post);

                    s = TextRenderer.MeasureText(fit, font);

                    // if still not fit then return "...\filename.ext"
                    if (s.Width > width)
                        fit = Path.Combine(EllipsisChars, post);
                }
                return fit;
            }
        }
    }

    /// <summary>
    /// Calculate exact sizes of strings
    /// </summary>
    public static class FontExtentions
    {
        /// <summary>
        /// Calculates the ascent in pixels of this font
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        public static float GetAscentPixels(this Font font)
        {
            var fontFamily = font.FontFamily;
            var ascent = fontFamily.GetCellAscent(font.Style);
            return font.Size * ascent / fontFamily.GetEmHeight(font.Style);
        }

        /// <summary>
        /// Calculates the descent in pixels of this font
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        public static float GetDescentPixels(this Font font)
        {
            var fontFamily = font.FontFamily;
            var descent = fontFamily.GetCellDescent(font.Style);
            return font.Size * descent / fontFamily.GetEmHeight(font.Style);
        }

        /// <summary>
        /// Calculates the distance from base line to base line of this font
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        public static float GetLineSpacingPixels(this Font font)
        {
            var fontFamily = font.FontFamily;
            var lineSpacing = fontFamily.GetLineSpacing(font.Style);
            return font.Size * lineSpacing / fontFamily.GetEmHeight(font.Style);
        }

        /// <summary>
        /// Calculates the maximum height in pixels of this font
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        public static float GetCellHeightPixels(this Font font)
        {
            var fontFamily = font.FontFamily;
            var ascent = fontFamily.GetCellAscent(font.Style);
            var descent = fontFamily.GetCellDescent(font.Style);
            return font.Size * (ascent + descent) / fontFamily.GetEmHeight(font.Style);
        }
    }
}