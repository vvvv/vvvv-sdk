using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Linq;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Utils;
using System.Collections;
using VVVV.Core.Runtime;
using VVVV.Core.Model.CS;
using VVVV.Core.Logging;

namespace VVVV.Core.Model
{
    public abstract class Project : PersistentIDContainer, IProject, IIDContainer, IRenameable
    {
        #region Converter classes
        
        protected class DocumentConverter : IConverter
        {
            protected Uri FLocation;
            
            public DocumentConverter(Uri location)
            {
                FLocation = location;
            }
            
            public bool Convert<TFrom, TTo>(TFrom fromItem, out TTo toItem)
            {
                toItem = default(TTo);
                
                if (fromItem is string)
                {
                    var path = fromItem as string;
                    var toType = typeof(TTo);
                    if (toType.IsAssignableFrom(typeof(IDocument)))
                    {
                        if (Directory.Exists(path))
                        {
                            // TODO: Handle directories
                            return false;
                        }
                        else
                        {
                            toItem = (TTo) DocumentFactory.CreateDocumentFromFile(path);

                            if (toItem != null)
                            {
                                return true;
                            }
                        }
                    }
                }
                
                return false;
            }
        }
        
        protected class ReferenceConverter : IConverter
        {
            protected Uri FLocation;
            
            public ReferenceConverter(Uri location)
            {
                FLocation = location;
            }
            
            public bool Convert<TFrom, TTo>(TFrom fromItem, out TTo toItem)
            {
                toItem = default(TTo);
                
                if (fromItem is string)
                {
                    var path = fromItem as string;
                    var toType = typeof(TTo);
                    if (toType.IsAssignableFrom(typeof(IReference)))
                    {
                        if (Directory.Exists(path))
                        {
                            // TODO: Handle directories
                            return false;
                        }
                        else if (File.Exists(path))
                        {
                            var extension = Path.GetExtension(path);
                            
                            // TODO: Move this to some kind of IReference factory
                            switch (extension) {
                                case ".dll":
                                    toItem = (TTo) (new AssemblyReference(Path.GetFullPath(path)) as IReference);
                                    return true;
                                case ".csproj":
                                    // TODO: Find the project via ID in solution
                                    return false;
                                default:
                                    return false;
                            }
                        }
                    }
                }
                
                return false;
            }
        }
        
        #endregion
        
        private BackgroundWorker FBackgroundWorker;
        protected EditableIDList<IDocument> FDocuments;
        protected EditableIDList<IReference> FReferences;
        protected DocumentConverter FDocumentConverter;
        protected ReferenceConverter FReferenceConverter;
        
        public Project(string name, Uri location)
            : base(name, location)
        {
            FReferenceConverter = new ReferenceConverter(location);
            FDocumentConverter = new DocumentConverter(location);
            
            FReferences = new EditableIDList<IReference>("References");
            FReferences.RootingChanged += FReferences_RootingChanged;
            Add(FReferences);

            FDocuments = new EditableIDList<IDocument>("Documents");
            FDocuments.RootingChanged += FDocuments_RootingChanged;
            Add(FDocuments);
            
            FReferences.Added += Reference_Added;
            FDocuments.Added += Document_Added;
            
            References = FReferences;
            Documents = FDocuments;
            
            FBackgroundWorker = new BackgroundWorker();
            FBackgroundWorker.WorkerReportsProgress = false;
            FBackgroundWorker.WorkerSupportsCancellation = false;
            
            FBackgroundWorker.DoWork += DoWorkCB;
            FBackgroundWorker.RunWorkerCompleted += RunWorkerCompletedCB;
        }

        void FReferences_RootingChanged(object sender, RootingChangedEventArgs args)
        {
            switch (args.Rooting) 
            {
                case RootingAction.Rooted:
                    FReferences.Mapper.RegisterMapping<IConverter>(FReferenceConverter);
                    break;
            }
        }
        
        void FDocuments_RootingChanged(object sender, RootingChangedEventArgs args)
        {
            switch (args.Rooting) 
            {
                case RootingAction.Rooted:
                    FDocuments.Mapper.RegisterMapping<IConverter>(FDocumentConverter);
                    break;
            }
        }
        
        void References_Changed(IViewableCollection<IReference> collection, IReference item)
        {
            IsDirty = true;
        }

        void Documents_Changed(IViewableCollection<IDocument> collection, IDocument item)
        {
            IsDirty = true;
        }
        
        protected override string CreateName(Uri location)
        {
            return Path.GetFileName(location.LocalPath);
        }
        
        public IEditableIDList<IDocument> Documents
        {
            get;
            private set;
        }

        public IEditableIDList<IReference> References
        {
            get;
            private set;
        }
        
        public Solution Solution
        {
            get;
            set;
        }
        
        public CompilerResults CompilerResults
        {
            get;
            set;
        }

        private string assemblyLocation;
        public virtual string AssemblyLocation
        {
            get
            {
                if (assemblyLocation == null)
                {
                    assemblyLocation = GenerateAssemblyLocation();
                }
                return assemblyLocation;
            }
        }

        public event CompiledEventHandler ProjectCompiledSuccessfully;
        public event CompiledEventHandler CompileCompleted;
        
        void Document_Added(IViewableCollection<IDocument> collection, IDocument item)
        {
            item.Project = this;
            if (item.IsDirty)
                item.Save();
        }
        
        void Reference_Added(IViewableCollection<IReference> collection, IReference item)
        {
            item.Project = this;
        }
        
        public string GenerateAssemblyLocation()
        {
            var assemblyBaseDir = Location.GetLocalDir().ConcatPath("bin").ConcatPath("Dynamic");
            
            int i = 0;
            var name = Location.LocalPath.GetHashCode().ToString();
            var assemblyName = string.Format("{0}._dynamic_.{1}.dll", name, i);
            var assemblyLocation = assemblyBaseDir.ConcatPath(assemblyName);
            while (File.Exists(assemblyLocation))
            {
                assemblyName = string.Format("{0}._dynamic_.{1}.dll", name, ++i);
                assemblyLocation = assemblyBaseDir.ConcatPath(assemblyName);
            }
            this.assemblyLocation = assemblyLocation;
            return assemblyLocation;
        }
        
        protected abstract CompilerResults DoCompile();
        
        public void Compile()
        {
            CompilerResults = DoCompile();
            
            var args = new CompilerEventArgs(CompilerResults);
            if (!CompilerResults.Errors.HasErrors)
            {
                this.assemblyLocation = CompilerResults.PathToAssembly;
                OnProjectCompiledSuccessfully(args);
            }
            
            OnCompileCompleted(args);
        }
        
        public virtual void CompileAsync()
        {
            if (FBackgroundWorker.IsBusy)
                return;
            
            FBackgroundWorker.RunWorkerAsync();
        }
        
        protected virtual void OnProjectCompiledSuccessfully(CompilerEventArgs args)
        {
            try
            {
                if (ProjectCompiledSuccessfully != null)
                    ProjectCompiledSuccessfully(this, args);
            }
            catch (Exception e)
            {
                Shell.Instance.Logger.Log(e);
            }
        }
        
        protected virtual void OnCompileCompleted(CompilerEventArgs args)
        {
            try
            {
                if (CompileCompleted != null)
                    CompileCompleted(this, args);
            }
            catch (Exception e)
            {
                Shell.Instance.Logger.Log(e);
            }
        }
        
        protected override void DisposeManaged()
        {
            FDocuments.Added -= Document_Added;
            FReferences.Added -= Reference_Added;
            
            Documents.Added -= Documents_Changed;
            Documents.Removed -= Documents_Changed;
            
            References.Added -= References_Changed;
            References.Removed -= References_Changed;
            
            FBackgroundWorker.DoWork -= DoWorkCB;
            FBackgroundWorker.RunWorkerCompleted -= RunWorkerCompletedCB;
            
            ProjectCompiledSuccessfully = null;
            CompileCompleted = null;
            
            foreach (var doc in Documents)
                doc.Dispose();
            
            FDocuments.Dispose();
            FReferences.Dispose();
            
            base.DisposeManaged();
        }
        
        private void DoWorkCB(object sender, DoWorkEventArgs args)
        {
            args.Result = DoCompile();
        }
        
        private void RunWorkerCompletedCB(object sender, RunWorkerCompletedEventArgs args)
        {
            CompilerResults = args.Result as CompilerResults;
            
            var compilerArgs = new CompilerEventArgs(CompilerResults);
            if (!CompilerResults.Errors.HasErrors)
            {
                this.assemblyLocation = CompilerResults.PathToAssembly;
                OnProjectCompiledSuccessfully(compilerArgs);
            }
            
            OnCompileCompleted(compilerArgs);
        }
        
		protected override void DoLoad()
		{
			Documents.Added += Documents_Changed;
            Documents.Removed += Documents_Changed;
            References.Added += References_Changed;
            References.Removed += References_Changed;
		}
        
        protected override void DoUnload()
        {
        	Documents.Added -= Documents_Changed;
            Documents.Removed -= Documents_Changed;
            References.Added -= References_Changed;
            References.Removed -= References_Changed;
        	
            foreach (var doc in Documents)
                doc.Unload();
            
            Documents.Clear();
            References.Clear();
        }
        
        public override void SaveTo(Uri newLocation)
        {
            var projectPath = newLocation.LocalPath;
            var projectDir = Path.GetDirectoryName(projectPath);
            
            // Create the project directory if it doesn't exist yet.
            if (!Directory.Exists(projectDir))
                Directory.CreateDirectory(projectDir);
            
            SaveDocumentsTo(newLocation);
            CopyReferencesTo(newLocation);
        }
        
        protected virtual void SaveDocumentsTo(Uri newLocation)
        {
            var projectDir = newLocation.GetLocalDir();
            
            // Save all documents to the new location.
            foreach (var document in Documents)
            {
                var destLocation = new Uri(projectDir + "/" + document.GetRelativePath());
                document.SaveTo(destLocation);
            }
        }
        
        protected virtual void CopyReferencesTo(Uri newLocation)
        {
            var projectDir = newLocation.GetLocalDir();
            
            // Copy all local references to the new location.
            foreach (var reference in References.Where((r) => !r.IsGlobal))
            {
                var relativePath = reference.GetRelativePath();
                
                Uri destLocation = null;
                if (!Path.IsPathRooted(relativePath))
                    destLocation = new Uri(projectDir + "/" + reference.GetRelativePath());
                else
                    destLocation = new Uri(relativePath);

                if (reference.AssemblyLocation != destLocation.LocalPath)
                {
                    File.Copy(reference.AssemblyLocation, destLocation.LocalPath, true);
                    var fileInfo = new FileInfo(destLocation.LocalPath);
                    fileInfo.IsReadOnly = false;
                }
            }
        }
    }
}
