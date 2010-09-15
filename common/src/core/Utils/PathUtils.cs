using System;

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
		public static string MakeRelativePath(string fromPath, string toPath)
		{
			if (!fromPath.EndsWith(@"\"))
				fromPath = fromPath + @"\";
			
			var fromUri = new Uri(fromPath);
			var toUri = new Uri(toPath);
			
			var relativeUri = fromUri.MakeRelativeUri(toUri);
			return relativeUri.ToString().Replace('/', '\\');
		}
	}
}
