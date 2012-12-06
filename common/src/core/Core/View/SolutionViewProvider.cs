using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;
using VVVV.Core.Commands;
using VVVV.Core.Dialogs;
using VVVV.Core.Menu;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.Core.View.Table;

namespace VVVV.Core.View
{
    /// <summary>
    /// Hides the Projects property and sorts projects by name.
    /// TODO: Work with project types other than CSharp.
    /// </summary>
    public class SolutionViewProvider : IParent, IAddMenuProvider, IDisposable//, IDroppable
    {
        protected Solution FSolution;
//        protected IDroppable FDroppable;
//        private IModelMapper FProjectsMapper;
        private readonly SortedViewableList<Project, string> FProjects;
        
        public SolutionViewProvider(Solution solution, ModelMapper mapper)
//            :base(solution.Projects, project => project.Name)
        {
            FSolution = solution;
            
            FSolution.Projects.ItemRenamed += FSolution_Projects_ItemRenamed;
//            
//            FProjectsMapper = mapper.CreateChildMapper(FSolution.Projects);
//            if (FProjectsMapper.CanMap<IDroppable>())
//                FDroppable = FProjectsMapper.Map<IDroppable>();

            FProjects = new SortedViewableList<Project, string>(solution.Projects, project => project.Name);
        }
        
        public void Dispose()
        {
            FSolution.Projects.ItemRenamed -= FSolution_Projects_ItemRenamed;
//            FProjectsMapper.Dispose();
            FProjects.Dispose();
        }

        void FSolution_Projects_ItemRenamed(INamed sender, string newName)
        {
            FProjects.UpdateKey(sender.Name, newName);
        }
        
//        public bool AllowDrop(Dictionary<string, object> items)
//        {
//            if (FDroppable != null)
//                return FDroppable.AllowDrop(items);
//            else
//                return false;
//        }
//        
//        public void DropItems(Dictionary<string, object> items, System.Drawing.Point pt)
//        {
//            if (FDroppable != null)
//                FDroppable.DropItems(items, pt);
//        }
        
        IEnumerable<IMenuEntry> IAddMenuProvider.GetEnumerator()
        {
            yield return new AddItemMenuEntry<Project>(FSolution.Projects, "New project",
                                                        Keys.Control | Keys.N, ProjectCreator);
        }

        protected Project ProjectCreator()
        {
            var dialog = new NameDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var name = dialog.EnteredText;
                var solutionDir = FSolution.Location.GetLocalDir();
                var projectPath = solutionDir.ConcatPath(name).ConcatPath(name + ".csproj");
                var location = new Uri(projectPath);
                var project = new CSProject(Path.GetFileName(location.LocalPath), location);
                if (File.Exists(project.Location.LocalPath))
                    project.Load();
                else
                    project.Save();
                return project;
            }
            else
                return null;
        }
        
        public IEnumerable Childs 
        {
            get 
            {
                return FProjects;
            }
        }
    }
}
