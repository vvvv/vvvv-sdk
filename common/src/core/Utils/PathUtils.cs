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
            Debug.Assert(!Path.HasExtension(basePath));
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
    }
}
