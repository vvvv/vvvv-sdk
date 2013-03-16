using System;
using System.Collections.Generic;
using System.IO;
using VVVV.Utils;

namespace VVVV.Core.Model
{
    public interface IDocument : IIDItem, IProjectItem
    {
        string LocalPath { get; }
        void SaveTo(string path);
    }
    
    public static class DocumentExtensionMethods
    {
        /// <summary>
        /// Returns the relative path from the specified persistent to this IPersistent.
        /// Example: Foo\Bar\ThisDocument.txt
        /// </summary>
        public static string GetRelativePath(this Uri location1, Uri location2)
        {
            var persistent2Dir = location2.GetLocalDir() + "\\";
            var relativePath = new Uri(persistent2Dir).MakeRelativeUri(location1).ToString();
            return Uri.UnescapeDataString(relativePath).Replace('/', '\\');
        }

    	/// <summary>
    	/// Returns the relative path from the containing IProject to this IDocument.
    	/// Example: Foo\Bar\ThisDocument.txt
    	/// </summary>
    	public static string GetRelativePath(this IDocument doc)
        {
            return PathUtils.MakeRelativePath(Path.GetDirectoryName(doc.Project.LocalPath), doc.LocalPath);
        }
    	
    	/// <summary>
    	/// Returns the relative path from the containing IProject to the directory of this IDocument.
    	/// Example: Foo\Bar
    	/// </summary>
    	public static string GetRelativeDir(this IDocument doc)
        {
            var relativePath = doc.GetRelativePath();
            if (string.IsNullOrEmpty(relativePath))
            	return string.Empty;
            else
            	return Path.GetDirectoryName(relativePath);
        }

        public static void Save(this IDocument doc)
        {
            doc.SaveTo(doc.LocalPath);
        }
    }
}
