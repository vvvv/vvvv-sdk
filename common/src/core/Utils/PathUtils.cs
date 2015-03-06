using System;
using System.IO;
using System.Diagnostics;

namespace VVVV.Utils
{
    /// <summary>
    /// Utility methods missing in the System.IO.Path class.
    /// </summary>
    public static class PathUtils
    {
        /// <summary>
        /// Returns the relative path to reach toPath from fromPath.
        /// </summary>
        public static string MakeRelativePath(string basePath, string absolutePath)
        {
            //Debug.Assert(!Path.HasExtension(basePath));
            var fromPath = basePath;
            if (basePath[basePath.Length - 1] != Path.DirectorySeparatorChar)
                fromPath = fromPath + Path.DirectorySeparatorChar;
            var toPath = Path.GetDirectoryName(absolutePath) + Path.DirectorySeparatorChar;
            
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);
            
            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Path.Combine(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar), Path.GetFileName(absolutePath));
            return Uri.UnescapeDataString(relativePath);
        }

        /// <summary>
        /// Whether or not dir2 is a subdirectory of dir1.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/5617320/given-full-path-check-if-path-is-subdirectory-of-some-other-path-or-otherwise</remarks>
        public static bool IsSubDir(string dir1, string dir2)
        {
            var di1 = new DirectoryInfo(dir1);
            var di2 = new DirectoryInfo(dir2);
            return IsSubDir(di1, di2);
        }

        /// <summary>
        /// Whether or not dir2 is a subdirectory of dir1.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/5617320/given-full-path-check-if-path-is-subdirectory-of-some-other-path-or-otherwise</remarks>
        public static bool IsSubDir(DirectoryInfo di1, DirectoryInfo di2)
        {
            while (di2.Parent != null)
            {
                if (di2.Parent.FullName == di1.FullName)
                    return true;
                else
                    di2 = di2.Parent;
            }
            return false;
        }

        /// <summary>
        /// Whether or not dir2 is a subdirectory of dir1 or they are the same.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/5617320/given-full-path-check-if-path-is-subdirectory-of-some-other-path-or-otherwise</remarks>
        public static bool IsSubOrSameDir(string dir1, string dir2)
        {
            var di1 = new DirectoryInfo(dir1);
            var di2 = new DirectoryInfo(dir2);
            return IsSubOrSameDir(di1, di2);
        }

        /// <summary>
        /// Whether or not dir2 is a subdirectory of dir1 or they are the same.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/5617320/given-full-path-check-if-path-is-subdirectory-of-some-other-path-or-otherwise</remarks>
        public static bool IsSubOrSameDir(DirectoryInfo di1, DirectoryInfo di2)
        {
            if (di1.FullName == di2.FullName)
                return true;
            return IsSubDir(di1, di2);
        }
    }
}
