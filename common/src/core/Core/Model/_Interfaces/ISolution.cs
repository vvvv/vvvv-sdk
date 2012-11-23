using System;
using System.Collections.Generic;
using System.IO;

using VVVV.Core;

namespace VVVV.Core.Model
{
    public interface ISolution : IIDContainer, IPersistent
    {
        IEditableIDList<IProject> Projects { get; }
        
        event CompiledEventHandler ProjectCompiledSuccessfully;
    }
    
    public static class SolutionExtensions
    {
    	/// <summary>
    	/// Finds the document with the specified filename. Looks through all documents in all projects
    	/// of this solution.
    	/// </summary>
    	/// <param name="filename">The filename where the document is located on the local filesystem.</param>
    	/// <returns>The document located at filename or null if not found.</returns>
    	public static IDocument FindDocument(this ISolution solution, string filename)
    	{
    		filename = filename.ToLower().Replace('/', '\\');
    		
    		foreach (var project in solution.Projects)
			{
    			var path = filename;
    			
    			if (!Path.IsPathRooted(path))
    				path = project.Location.GetLocalDir().ConcatPath(filename).ToLower();
    			
    			foreach (var document in project.Documents)
    			{
    				if (document.Location.LocalPath.ToLower() == path)
    					return document;
    			}
			}
    		
    		return null;
    	}
    	
    	public static IProject FindProject(this ISolution solution, string filename)
    	{
    		foreach (var project in solution.Projects)
    		{
    			if (project.Location.LocalPath == filename)
    				return project;
    		}
    		
    		return null;
    	}
    }
}
