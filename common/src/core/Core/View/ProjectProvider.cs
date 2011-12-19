using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using VVVV.HDE.Model;
using VVVV.HDE.Model.CS;
using VVVV.HDE.Viewer.Model;
using VVVV.Utils.Event;
using VVVV.Utils.Menu;
using VVVV.Utils;

namespace VVVV.HDE.Model.Provider
{
    public class ProjectProvider: Disposable, 
        ITreeContentProvider, ILabelProvider, IContextMenuProvider, IDragDropProvider, 
        ISubscriber<EventSubject<PropertyChangedEventArgs>>
    {
        private Dictionary<IProject, ReferenceProvider.ProjectContainer> FContainerCache = new Dictionary<IProject, ReferenceProvider.ProjectContainer>();
        public event EventHandler ContentChanged;
        public event EventHandler LabelChanged;
        
        public ProjectProvider(IEventHub eventHub)
        {
            // We're only interested in property changed events coming from IProject.
            eventHub.Subscribe<EventSubject<PropertyChangedEventArgs>>(this, (x) => { return x.Sender is IProject; });
        }
        
        public void Receive(EventSubject<PropertyChangedEventArgs> subject)
        {
            if (subject.Args.PropertyName == "Name")
                OnLabelChanged(subject.Sender);
            else
                OnContentChanged(subject.Sender);
        }
        
        public virtual IEnumerable GetChildren(object element)
        {
            var project = element as IProject;
            var documents = project.Documents.ToArray();
            var children = new object[documents.Length + 1];
            Array.Copy(documents, 0, children, 1, documents.Length);
            if (FContainerCache.ContainsKey(project))
                children[0] = FContainerCache[project];
            else
            {
                var pc = new ReferenceProvider.ProjectContainer(project);
                children[0] = pc;
                FContainerCache.Add(project, pc);
            }
            return children;
        }
        
        public virtual string GetText(object element)
        {
            var project = element as IProject;
            return project.Name;
        }
        
        public string GetToolTip(object element)
        {
            return "";
        }
        
        public virtual IMenuEntry GetContextMenu(object element)
        {
            var menu = new MenuEntry();
            var newMenu = new MenuEntry("New");
            var docMenu = new MenuEntry("Document");
            docMenu.UserData = element;
            docMenu.OnMenuItemClicked += MenuItemClickedCB;
            menu.Add(newMenu);
            newMenu.Add(docMenu);
            return menu;
        }
        
        protected virtual void OnContentChanged(object sender)
        {
            if (ContentChanged != null)
                ContentChanged(sender, EventArgs.Empty);
        }
        
        protected virtual void OnLabelChanged(object sender)
        {
            if (LabelChanged != null)
                LabelChanged(sender, EventArgs.Empty);
        }
        
        protected virtual void MenuItemClickedCB(object sender, EventArgs args)
        {
            var menuItem = sender as MenuEntry;
            var project = menuItem.UserData as IProject;
            var location = new Uri("C:\\bla.cs");
            project.Add(new TextDocument(project, location));
        }
        
        public bool AllowDrag(object element)
        {
            return true;
        }
        
        public object DragItem(object element)
        {
            return (element as IProject).Name;
        }
        
        public bool AllowDrop(object element, Dictionary<string, object> dropItems)
        {
            foreach (KeyValuePair<string, object> pair in dropItems)
            {
                var pathArray = pair.Value as string[];
                if (pathArray != null)
                {
                    try
                    {
                        var attr = File.GetAttributes(pathArray[0]);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            // TODO: Handly directories
                            return false;
                        }
                        else
                        {
                            // Valid file
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
                }
            }
            
            return false;
        }
        
        public void DropItem(object element, Dictionary<string, object> dropItems)
        {
            var project = element as IProject;
            var documents = new List<IDocument>();
            
            foreach (KeyValuePair<string, object> pair in dropItems)
            {
                var pathArray = pair.Value as string[];
                if (pathArray != null)
                {
                    foreach (var path in pathArray)
                    {
                        try
                        {
                            var attr = File.GetAttributes(path);
                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                // TODO: Handly directories
                            }
                            else
                            {
                                // TODO: We need some kind of factory to create the right type of IDocument.
                                var location = new Uri(path);
                                var doc = new TextDocument(project, location);
                                documents.Add(doc);
                            }
                        }
                        catch (Exception)
                        {
                            // Ignore
                        }
                    }
                }
                
                if (documents.Count > 0)
                {
                    project.AddRange(documents);
                    break;
                }
            }
        }
    }
}
