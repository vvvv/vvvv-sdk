using System;
using System.Collections.Generic;
using System.IO;

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
    }
}
