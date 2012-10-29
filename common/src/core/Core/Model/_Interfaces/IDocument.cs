using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace VVVV.Core.Model
{
    public interface IDocument : IIDItem, IProjectItem, IPersistent
    {
    }
    
    public static class DocumentExtensionMethods
    {
    	/// <summary>
    	/// Returns the relative path from the containing IProject to this IDocument.
    	/// Example: Foo\Bar\ThisDocument.txt
    	/// </summary>
    	public static string GetRelativePath(this IDocument doc)
        {
    		return doc.GetRelativePath(doc.Project);
        }

        /// <summary>
        /// Returns the relative path from the given IProject to this IDocument.
        /// Example: Foo\Bar\ThisDocument.txt
        /// </summary>
        public static string GetRelativeDir(this IDocument doc, IProject project)
        {
            var relativePath = doc.GetRelativePath(project);
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;
            else
                return Path.GetDirectoryName(relativePath);
        }
    	
    	/// <summary>
    	/// Returns the relative path from the containing IProject to the directory of this IDocument.
    	/// Example: Foo\Bar
    	/// </summary>
    	public static string GetRelativeDir(this IDocument doc)
        {
            Debug.Assert(doc.Project != null); // Document must be rooted
            return doc.GetRelativeDir(doc.Project);
        }
    }
}
