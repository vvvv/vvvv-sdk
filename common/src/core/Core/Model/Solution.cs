using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using ICSharpCode.SharpDevelop.Dom;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Runtime;
using VVVV.Core.Commands;

namespace VVVV.Core.Model
{
    public class Solution : PersistentIDContainer, IIDContainer
    {
        private readonly ServiceProvider FServiceProvider;
    	
    	public Solution(Uri location, IServiceProvider serviceProvider)
    	    : base(Path.GetFileName(location.LocalPath), location, true)
        {
            FServiceProvider = new ServiceProvider(serviceProvider);
            if (Shell.Instance.IsRuntime)
                FServiceProvider.RegisterService<ICommandHistory>(new CommandHistory(FServiceProvider));
            else
                FServiceProvider.RegisterService<ICommandHistory>(new HDECommandHistory(this));
            
            // Do not allow rename on add. Rename triggers save/delete in case of PersistentIDContainer.
            Projects = new EditableIDList<Project>("Projects", false);
            Add(Projects);
            
            ProjectContentRegistry = new ProjectContentRegistry();
            var tempPath = Shell.TempPath.ConcatPath("ProjectContentRegistry");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            ProjectContentRegistry.ActivatePersistence(tempPath);

            Projects.Added += Projects_Added;
            Projects.Removed += Projects_Removed;
            
            OnRootingChanged(RootingAction.Rooted);
        }

        public override IServiceProvider ServiceProvider
        {
            get
            {
                return FServiceProvider;
            }
        }
    	
        public event CompiledEventHandler ProjectCompiledSuccessfully;
        
        public IEditableIDList<Project> Projects 
        { 
            get;
            private set;
        }
        
        public ProjectContentRegistry ProjectContentRegistry
        {
        	get;
        	private set;
        }
        
        void Projects_Added(IViewableCollection<Project> collection, Project project)
        {
        	project.Solution = this;
            project.ProjectCompiledSuccessfully += Project_Compiled;
        }

        void Projects_Removed(IViewableCollection<Project> collection, Project project)
        {
            project.ProjectCompiledSuccessfully -= Project_Compiled;
        }
        
        void Project_Compiled(object sender, CompilerEventArgs args)
        {
            // Refire this event
            OnProjectCompiledSuccessfully(sender, args);
        }
        
        protected virtual void OnProjectCompiledSuccessfully(object sender, CompilerEventArgs args)
        {
            if (ProjectCompiledSuccessfully != null)
                ProjectCompiledSuccessfully(sender, args);
        }
        
        protected override void DisposeManaged()
        {
            ProjectCompiledSuccessfully = null;
            base.DisposeManaged();
        }
        
		protected override string CreateName(Uri location)
		{
			return Path.GetFileNameWithoutExtension(location.LocalPath);
		}
        
        public override void SaveTo(Uri location)
        {
            // TODO: Implement this
        }
        
		protected override void DoLoad()
		{
			// TODO: Implement this
		}
		
		protected override void DoUnload()
		{
			// TODO: Implement this
		}

        /// <summary>
        /// Finds the document with the specified filename. Looks through all documents in all projects
        /// of this solution.
        /// </summary>
        /// <param name="filename">The filename where the document is located on the local filesystem.</param>
        /// <returns>The document located at filename or null if not found.</returns>
        public IDocument FindDocument(string filename)
        {
            filename = filename.ToLower().Replace('/', '\\');

            foreach (var project in Projects)
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

        public IProject FindProject(string filename)
        {
            foreach (var project in Projects)
            {
                if (project.Location.LocalPath == filename)
                    return project;
            }

            return null;
        }
    }
}
