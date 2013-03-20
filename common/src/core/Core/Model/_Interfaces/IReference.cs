using System;
using System.Collections.Generic;
using System.IO;

namespace VVVV.Core.Model
{
    /// <summary>
    /// IReference is used by IProject to reference other assemblies or projects.
    /// </summary>
    public interface IReference : IIDItem, IProjectItem
    {
        /// <summary>
        /// The full path to the referenced assembly.
        /// </summary>
        string AssemblyLocation 
        {
            get; 
        }
        
        /// <summary>
        /// Determines whether this reference is a global reference (for example
        /// in the global assembly cache). Non global references will be copied
        /// to the new location in a IProject.SaveTo() operation.
        /// </summary>
        bool IsGlobal
        {
        	get;
        }
    }
    
    public static class ReferenceExtensionMethods
    {
    	/// <summary>
    	/// Returns the relative path from the specified project to this IReference.
    	/// Example: Foo\Bar\ThisReference.dll
    	/// </summary>
    	public static string GetRelativePath(this IReference reference, IProject project)
        {
    		var projectDir = Path.GetDirectoryName(project.LocalPath) + "\\";
    		var relativePath = new Uri(projectDir).MakeRelativeUri(new Uri(reference.AssemblyLocation)).ToString();
            return Uri.UnescapeDataString(relativePath).Replace('/', '\\');
        }
    	
    	/// <summary>
    	/// Returns the relative path from the containing IProject to this IReference.
    	/// Example: Foo\Bar\ThisReference.dll
    	/// </summary>
    	public static string GetRelativePath(this IReference reference)
        {
    		return reference.GetRelativePath(reference.Project);
        }
    	
    	/// <summary>
    	/// Returns the relative path from the containing IProject to the directory of this IReference.
    	/// Example: Foo\Bar
    	/// </summary>
    	public static string GetRelativeDir(this IReference reference)
        {
            var relativePath = reference.GetRelativePath();
            if (string.IsNullOrEmpty(relativePath))
            	return string.Empty;
            else
            	return Path.GetDirectoryName(relativePath);
        }
    }
}
