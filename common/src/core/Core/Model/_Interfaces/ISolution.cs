using System;
using System.Collections.Generic;
using System.IO;

using VVVV.Core;

namespace VVVV.Core.Model
{
    public interface ISolution : IIDContainer
    {
        IEditableIDList<IProject> Projects { get; }
        
        event CompiledEventHandler ProjectCompiledSuccessfully;

        string LocalPath { get; }
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
                    path = Path.GetDirectoryName(project.LocalPath).ConcatPath(filename);
                
                foreach (var document in project.Documents)
                {
                    if (string.Compare(document.LocalPath, path, StringComparison.InvariantCultureIgnoreCase) == 0)
                        return document;
                }
            }
            
            return null;
        }
        
        public static IProject FindProject(this ISolution solution, string filename)
        {
            foreach (var project in solution.Projects)
            {
                if (string.Compare(project.LocalPath, filename, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return project;
            }
            
            return null;
        }
    }
}
