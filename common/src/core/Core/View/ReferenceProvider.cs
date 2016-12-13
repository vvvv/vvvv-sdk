using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using Microsoft.Practices.Unity;

using VVVV.HDE.Viewer.Model;
using VVVV.HDE.Commands;
using VVVV.Utils;
using VVVV.Utils.Menu;
using VVVV.Utils.Command;

namespace VVVV.HDE.Model.Provider
{
    public class ReferenceProvider: Disposable, ITreeContentProvider, ILabelProvider, IContextMenuProvider
    {
        public class ProjectContainer
        {
            public IProject Project { get; private set; }
            public ProjectContainer(IProject project)
            {
                Project = project;
            }
        }
        
        private ICommandHistory FHistory;
        
        public event EventHandler ContentChanged;
        
        protected virtual void OnContentChanged(object sender, EventArgs e)
        {
            if (ContentChanged != null) {
                ContentChanged(sender, e);
            }
        }
        public event EventHandler LabelChanged;
        
        protected virtual void OnLabelChanged(object sender, EventArgs e)
        {
            if (LabelChanged != null) {
                LabelChanged(sender, e);
            }
        }
        
        public ReferenceProvider(ICommandHistory history)
        {
            FHistory = history;
        }
        
        public IEnumerable GetChildren(object element)
        {
            var result = new List<IReference>();
            if (element is ProjectContainer)
            {
                var container = element as ProjectContainer;
                var project = container.Project;
                
                foreach (var reference in project.References)
                    result.Add(reference);
                
                result.Sort((x, y) => {return x.Name.CompareTo(y.Name);});
            }
            return result;
        }
        
        public string GetText(object element)
        {
            if (element is ProjectContainer)
                return "References";
            
            var container = element as IReference;
            return container.Name;
        }
        
        public string GetToolTip(object element)
        {
            return "";
        }
        
        public IMenuEntry GetContextMenu(object element)
        {
            if (element is ProjectContainer)
            {
                var menu = new MenuEntry();
                var addReferenceItem = new MenuEntry("Add Reference to Assembly");
                addReferenceItem.UserData = (element as ProjectContainer).Project;
                addReferenceItem.OnMenuItemClicked += new EventHandler(AddReferenceItemClickedCB);

                var addProjectItem = new MenuEntry("Add Reference to Project");
                addProjectItem.UserData = (element as ProjectContainer).Project;
                addProjectItem.OnMenuItemClicked += new EventHandler(AddProjectItemClickedCB);

                menu.Add(addReferenceItem);
                menu.Add(addProjectItem);
                return menu;
            }
            else
            {
                return new MenuEntry();
            }
        }

        //click on new assembly reference
        private void AddReferenceItemClickedCB(object sender, EventArgs args)
        {
            var menuItem = sender as MenuEntry;

            if (menuItem.UserData is IProject)
            {
                var proj = menuItem.UserData as IProject;

                //load assembly
                var fd = new OpenFileDialog();
                fd.DefaultExt = "dll";
                fd.Filter = "Assembly (*.dll)|*.dll";
                fd.ShowDialog();
                if (fd.FileName == "") return;

                var com = new CommandAddAssemblyReference(new Uri(fd.FileName), proj);
                FHistory.Insert(com);
            }
        }

        //click on new project reference
        private void AddProjectItemClickedCB(object sender, EventArgs args)
        {
            var menuItem = sender as MenuEntry;

            if (menuItem.UserData is IProject)
            {
                var proj = menuItem.UserData as IProject;

                MessageBox.Show("Add Reference to Project clicked");

                //TODO: command does not yet exist
                //var com = new CommandAddProjectReference( "nameOfReferenceProject", proj.Name);
                //Root.History.Insert(com);
            }
        }
    }
}
