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
using VVVV.Utils;

namespace VVVV.Core.View
{
    // TODO: work with all kind of documents (IAddMenuProvider)
    public class ProjectViewProvider : IViewableList, IDroppable, IAddMenuProvider, IDisposable, IDescripted, INamed, IRenameable
    {
        #region Folder class
        
        protected class Folder : IViewableList, INamed, IMenuEntry, IDisposable
        {
            public string Name
            {
                get;
                private set;
            }
            
            public SortedEditableList<Folder, string> Folders
            {
                get;
                private set;
            }
            
            public SortedEditableList<IDocument, string> Documents
            {
                get;
                private set;
            }
            
            public Folder Parent
            {
                get;
                protected set;
            }
            
            public Folder(string name)
            {
                Name = name;
                Folders = new SortedEditableList<Folder, string>(folder => folder.Name);
                Documents = new SortedEditableList<IDocument, string>(document => document.Name);
                
                Folders.Added += Folders_Added;
                Folders.Removed += Folders_Removed;
                Folders.OrderChanged += Folders_OrderChanged;
                Documents.Added += Documents_Added;
                Documents.Removed += Documents_Removed;
                Documents.OrderChanged += Documents_OrderChanged;
            }
            
            public void Dispose()
            {
                Folders.Added -= Folders_Added;
                Folders.Removed -= Folders_Removed;
                Folders.OrderChanged -= Folders_OrderChanged;
                Documents.Added -= Documents_Added;
                Documents.Removed -= Documents_Removed;
                Documents.OrderChanged -= Documents_OrderChanged;
                
                foreach (var folder in Folders)
                    folder.Dispose();
                
                Folders.Dispose();
                Documents.Dispose();
            }

            void Documents_OrderChanged(IViewableList<IDocument> list)
            {
                OnOrderChanged();
            }

            void Folders_OrderChanged(IViewableList<Folder> list)
            {
                OnOrderChanged();
            }

            void Documents_Removed(IViewableCollection<IDocument> collection, IDocument item)
            {
                OnRemoved(item);
            }

            void Folders_Removed(IViewableCollection<Folder> collection, Folder item)
            {
                OnRemoved(item);
            }

            void Documents_Added(IViewableCollection<IDocument> collection, IDocument item)
            {
                OnAdded(item);
            }

            void Folders_Added(IViewableCollection<Folder> collection, Folder item)
            {
                item.Parent = this;
                
                OnAdded(item);
            }
            
            public IEnumerator GetEnumerator()
            {
                foreach (var folder in Folders) {
                    yield return folder;
                }
                foreach (var doc in Documents) {
                    yield return doc;
                }
            }
            
            public event RenamedHandler Renamed;
            
            protected virtual void OnRenamed(string newName)
            {
                if (Renamed != null) {
                    Renamed(this, newName);
                }
            }
            
            public override string ToString()
            {
                return string.Format("[Folder] {0}", Name);
            }
            
            public event OrderChangedHandler OrderChanged;
            
            protected virtual void OnOrderChanged()
            {
                if (OrderChanged != null) {
                    OrderChanged(this);
                }
            }
            
            public event CollectionDelegate Added;
            
            protected virtual void OnAdded(object item)
            {
                if (Added != null) {
                    Added(this, item);
                }
            }
            
            public event CollectionDelegate Removed;
            
            protected virtual void OnRemoved(object item)
            {
                if (Removed != null) {
                    Removed(this, item);
                }
            }
            
            public event CollectionUpdateDelegate Cleared;
            
            protected virtual void OnCleared(IViewableCollection collection)
            {
                if (Cleared != null) {
                    Cleared(collection);
                }
            }
            
            public event CollectionUpdateDelegate UpdateBegun;
            
            protected virtual void OnUpdateBegun(IViewableCollection collection)
            {
                if (UpdateBegun != null) {
                    UpdateBegun(collection);
                }
            }
            
            public event CollectionUpdateDelegate Updated;
            
            protected virtual void OnUpdated(IViewableCollection collection)
            {
                if (Updated != null) {
                    Updated(collection);
                }
            }
            
            public object this[int index]
            {
                get
                {
                    if (index < Folders.Count)
                        return Folders[index];
                    
                    index -= Folders.Count;
                    return Documents[index];
                }
            }
            
            public int Count
            {
                get
                {
                    return Folders.Count + Documents.Count;
                }
            }
            
            public bool Contains(object item)
            {
                return ((IViewableCollection) Folders).Contains(item) || ((IViewableCollection) Documents).Contains(item);
            }
            
            #region IMenuEntry Members
            
            public System.Windows.Forms.Keys ShortcutKeys
            {
                get
                {
                    return Keys.None;
                }
            }
            
            public bool Enabled
            {
                get
                {
                    return true;
                }
            }
            
            public void Click()
            {
                // Nothing to do here
            }
            
            IEnumerator<IMenuEntry> IEnumerable<IMenuEntry>.GetEnumerator()
            {
                yield break;
            }
            
            #endregion
        }
        
        #endregion
        
        #region FolderContextMenuProvider
        
        protected class FolderContextMenuProvider
        {
        }
        
        #endregion
        
        protected IProject FProject;
        protected IDroppable FDroppable;
        protected Folder FRootFolder;
        protected Dictionary<string, Folder> FFolderMap;
        private ModelMapper FDocumentsMapper;
        
        public ProjectViewProvider(IProject project, ModelMapper mapper)
        {
            FProject = project;
            
            FDocumentsMapper = mapper.CreateChildMapper(FProject.Documents);
            if (FDocumentsMapper.CanMap<IDroppable>())
                FDroppable = FDocumentsMapper.Map<IDroppable>();
            
            // Create a fake model, which shows documents in folders
            FFolderMap = new Dictionary<string, Folder>();
            
            // First create our root folder.
            FRootFolder = new Folder(string.Empty);
            FFolderMap[FRootFolder.Name] = FRootFolder;
            
            // Now create all folders.
            foreach (var doc in FProject.Documents)
                FProject_Documents_Added(FProject.Documents, doc);
            
            // Keep changes in project in sync with our folder model.
            FProject.Documents.Added += FProject_Documents_Added;
            FProject.Documents.Removed += FProject_Documents_Removed;
            FProject.Documents.ItemRenamed += FProject_ItemRenamed;
            FProject.References.Added += HandleProjectReferenceAdded;
            FProject.References.Removed += HandleProjectReferenceRemoved;
            
            // Keep us in sync with the root folder.
            FRootFolder.Added += FRootFolder_Added;
            FRootFolder.Removed += FRootFolder_Removed;
            FRootFolder.OrderChanged += FRootFolder_OrderChanged;

            FProject.Renamed += FProject_Renamed;
        }
        
        public void Dispose()
        {
            FProject.Renamed -= FProject_Renamed;
            FProject.Documents.Added -= FProject_Documents_Added;
            FProject.Documents.Removed -= FProject_Documents_Removed;
            FProject.Documents.ItemRenamed -= FProject_ItemRenamed;
            FProject.References.Added -= HandleProjectReferenceAdded;
            FProject.References.Removed -= HandleProjectReferenceRemoved;
            FRootFolder.Added -= FRootFolder_Added;
            FRootFolder.Removed -= FRootFolder_Removed;
            FRootFolder.OrderChanged -= FRootFolder_OrderChanged;
            FRootFolder.Dispose();
            if (FDocumentsMapper != null)
                FDocumentsMapper.Dispose();
        }
        
        void HandleProjectReferenceAdded(IViewableCollection<IReference> collection, IReference item)
        {
            FProject.Save();
        }

        void HandleProjectReferenceRemoved(IViewableCollection<IReference> collection, IReference item)
        {
            FProject.Save();
        }

        protected Folder GetOrCreateFolder(string relativePath)
        {
            if (FFolderMap.ContainsKey(relativePath))
                return FFolderMap[relativePath];
            
            // path is : foo/var/lar/sar
            // let's say we only have foo/var
            // we need to find foo/var, create lar and add it to var, create sar and add it to sar.
            var parentFolder = FRootFolder;
            var tokens = relativePath.Split(Path.DirectorySeparatorChar);
            var path = FRootFolder.Name;
            foreach (var token in tokens)
            {
                path = Path.Combine(path, token);
                if (FFolderMap.ContainsKey(path))
                    parentFolder = FFolderMap[path];
                else
                {
                    parentFolder = CreateFolder(parentFolder, token);
                    FFolderMap[path] = parentFolder;
                }
            }
            
            return parentFolder;
        }
        
        protected Folder CreateFolder(Folder parentFolder, string name)
        {
            var folder = new Folder(name);
            parentFolder.Folders.Add(folder);
            return folder;
        }
        
        protected void TryRemoveFolder(Folder folder)
        {
            if (folder != FRootFolder && folder.Count == 0)
            {
                folder.Parent.Folders.Remove(folder);
                FFolderMap.Remove(folder.Name);
            }
        }

        void FProject_Renamed(INamed sender, string newName)
        {
            OnRenamed(Path.GetFileName(newName));
        }
        
        void FProject_Documents_Removed(IViewableCollection<IDocument> collection, IDocument item)
        {
            var relativeDir = item.GetRelativeDir();
            var folder = GetOrCreateFolder(relativeDir);
            folder.Documents.Remove(item);
            TryRemoveFolder(folder);
        }

        void FProject_Documents_Added(IViewableCollection<IDocument> collection, IDocument item)
        {
            var relativeDir = item.GetRelativeDir();
            var folder = GetOrCreateFolder(relativeDir);
            folder.Documents.Add(item);
        }
        
        void FProject_ItemRenamed(INamed sender, string newName)
        {
            var doc = sender as IDocument;
            var folder = GetOrCreateFolder(doc.GetRelativeDir());
            folder.Documents.UpdateKey(doc.Name, newName);
        }
        
        void FRootFolder_OrderChanged(IViewableList list)
        {
            OnOrderChanged();
        }

        void FRootFolder_Removed(IViewableCollection collection, object item)
        {
            if (Removed != null)
                Removed(this, item);
        }

        void FRootFolder_Added(IViewableCollection collection, object item)
        {
            if (Added != null)
                Added(this, item);
        }
        
        public int Count
        {
            get
            {
                return FRootFolder.Count + 1;
            }
        }
        
        public object this[int index]
        {
            get
            {
                if (index == 0)
                    return FProject.References;
                return FRootFolder[index - 1];
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return FProject.References;
            foreach (var folder in FRootFolder) {
                yield return folder;
            }
        }
        
        public event OrderChangedHandler OrderChanged;
        
        protected virtual void OnOrderChanged()
        {
            if (OrderChanged != null)
                OrderChanged(this);
        }
        
        public event CollectionDelegate Added;
        
        protected virtual void OnAdded(object item)
        {
            if (Added != null) {
                Added(this, item);
            }
        }
        
        public event CollectionDelegate Removed;
        
        protected virtual void OnRemoved(object item)
        {
            if (Removed != null) {
                Removed(this, item);
            }
        }
        
        public event CollectionUpdateDelegate Cleared;
        
        protected virtual void OnCleared()
        {
            if (Cleared != null) {
                Cleared(this);
            }
        }
        
        public event CollectionUpdateDelegate UpdateBegun;
        
        protected virtual void OnUpdateBegun(IViewableCollection collection)
        {
            if (UpdateBegun != null) {
                UpdateBegun(collection);
            }
        }
        
        public event CollectionUpdateDelegate Updated;
        
        protected virtual void OnUpdated(IViewableCollection collection)
        {
            if (Updated != null) {
                Updated(collection);
            }
        }
        
        public bool Contains(object item)
        {
            if (item == FProject.References)
                return true;
            return FRootFolder.Contains(item);
        }
        
        public bool AllowDrop(Dictionary<string, object> items)
        {
            if (FDroppable != null)
                return FDroppable.AllowDrop(items);
            else
                return false;
        }
        
        public void DropItems(Dictionary<string, object> items, System.Drawing.Point pt)
        {
            if (FDroppable != null)
                FDroppable.DropItems(items, pt);
        }
        
        IEnumerable<IMenuEntry> IAddMenuProvider.GetEnumerator()
        {
            yield return new AddItemMenuEntry<IDocument>(FProject.Documents, "New document",
                                                         Keys.Control | Keys.N, DocumentCreator);
            yield return new AddItemMenuEntry<IDocument>(FProject.Documents, "Existing document",
                                                         Keys.None, ExistingDocumentCreator);
        }

        protected IDocument DocumentCreator()
        {
            var dialog = new NameDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var name = dialog.EnteredText;
                var projectDir = Path.GetDirectoryName(FProject.LocalPath);
                var documentPath = projectDir.ConcatPath(name);
                var document = DocumentFactory.CreateDocumentFromFile(documentPath);
                if (!File.Exists(document.LocalPath))
                    document.SaveTo(document.LocalPath);
                return document;
            }
            else
                return null;
        }
        
        protected IDocument ExistingDocumentCreator()
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = Path.GetDirectoryName(FProject.LocalPath);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return DocumentFactory.CreateDocumentFromFile(dialog.FileName);
            }
            else
                return null;
        }
        
        public string Description 
        {
            get
            {
                return FProject.LocalPath;
            }
        }

        #region INamed
        
        public string Name
        {
            get { return Path.GetFileName(FProject.LocalPath); }
            set
            {
                if (value != Name)
                {
                    var path = Path.Combine(Path.GetDirectoryName(FProject.LocalPath), value);
                    FProject.SaveTo(path);
                    FProject.Solution.Projects.Remove(FProject);
                    File.Delete(FProject.LocalPath);
                    FProject.Dispose();
                    // File factory will pick up the new one ...
                }
            }
        }

        public event RenamedHandler Renamed;

        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null)
            {
                Renamed(this, newName);
            }
        }

        #endregion

        public bool CanRenameTo(string value)
        {
            return value.EndsWith("proj");
        }
    }
}
