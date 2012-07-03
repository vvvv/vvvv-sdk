using System;
using System.IO;

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
			var fromPath = Path.GetDirectoryName(basePath) + @"\";
			var toPath = Path.GetDirectoryName(absolutePath) + @"\";
			
			var fromUri = new Uri(fromPath);
			var toUri = new Uri(toPath);
			
			var relativeUri = fromUri.MakeRelativeUri(toUri);
			var relativePath = Path.Combine(relativeUri.ToString().Replace('/', '\\'), Path.GetFileName(absolutePath));
			return Uri.UnescapeDataString(relativePath);
		}
	}
}
