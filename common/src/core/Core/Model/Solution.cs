using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using ICSharpCode.SharpDevelop.Dom;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Runtime;

namespace VVVV.Core.Model
{
    public class Solution : IDContainer, ISolution, IIDContainer
    {
        private MappingRegistry FRegistry;
        
        public Solution(string path, MappingRegistry registry)
            : base(Path.GetFileName(path), true)
        {
            LocalPath = path;
            FRegistry = registry;
            Mapper = new ModelMapper(this, registry);
            
            // Do not allow rename on add. Rename triggers save/delete in case of PersistentIDContainer.
            Projects = new EditableIDList<IProject>("Projects", false);
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
        
        public event CompiledEventHandler ProjectCompiledSuccessfully;
        
        public IEditableIDList<IProject> Projects 
        { 
            get;
            private set;
        }
        
        public ProjectContentRegistry ProjectContentRegistry
        {
            get;
            private set;
        }
        
        void Projects_Added(IViewableCollection<IProject> collection, IProject project)
        {
            project.Solution = this;
            project.ProjectCompiledSuccessfully += Project_Compiled;
        }

        void Projects_Removed(IViewableCollection<IProject> collection, IProject project)
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

        public string LocalPath
        {
            get;
            private set;
        }
    }
}
