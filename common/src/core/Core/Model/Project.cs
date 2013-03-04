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
    public abstract class Project : IDContainer, IProject, IIDContainer
    {
        #region Converter classes
        
        protected class DocumentConverter : IConverter
        {
            protected string FLocalPath;
            
            public DocumentConverter(string path)
            {
                FLocalPath = path;
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
            protected string FLocalPath;

            public ReferenceConverter(string location)
            {
                FLocalPath = location;
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
        
        public Project(string path)
            : base(path)
        {
            LocalPath = path;

            FReferenceConverter = new ReferenceConverter(path);
            FDocumentConverter = new DocumentConverter(path);
            
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
        
        public ISolution Solution
        {
            get;
            set;
        }
        
        public CompilerResults CompilerResults
        {
            get;
            set;
        }

        public event CompiledEventHandler ProjectCompiledSuccessfully;
        public event CompiledEventHandler CompileCompleted;
        
        void Document_Added(IViewableCollection<IDocument> collection, IDocument item)
        {
            item.Project = this;
        }
        
        void Reference_Added(IViewableCollection<IReference> collection, IReference item)
        {
            item.Project = this;
        }
        
        protected abstract CompilerResults DoCompile();
        
        public void Compile()
        {
            CompilerResults = DoCompile();
            OnCompileCompleted(new CompilerEventArgs(CompilerResults));
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
                if (!args.CompilerResults.Errors.HasErrors)
                    OnProjectCompiledSuccessfully(args);
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
            OnCompileCompleted(new CompilerEventArgs(CompilerResults));
        }

        public void Save()
        {
            SaveTo(LocalPath);
        }
        
        public virtual void SaveTo(string projectPath)
        {
            var projectDir = Path.GetDirectoryName(projectPath);
            
            // Create the project directory if it doesn't exist yet.
            if (!Directory.Exists(projectDir))
                Directory.CreateDirectory(projectDir);

            SaveDocumentsTo(projectDir);
            CopyReferencesTo(projectDir);
        }
        
        protected virtual void SaveDocumentsTo(string projectDir)
        {
            // Save all documents to the new location.
            foreach (var document in Documents)
            {
                var path = Path.Combine(projectDir, document.GetRelativePath());
                document.SaveTo(path);
            }
        }

        protected virtual void CopyReferencesTo(string projectDir)
        {
            // Copy all local references to the new location.
            foreach (var reference in References.Where((r) => !r.IsGlobal))
            {
                var relativePath = reference.GetRelativePath();
                
                string path = null;
                if (!Path.IsPathRooted(relativePath))
                    path = Path.Combine(projectDir, reference.GetRelativePath());
                else
                    path = relativePath;
                path = Path.GetFullPath(path);
                if (string.Compare(reference.AssemblyLocation, path, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    try
                    {
                        File.Copy(reference.AssemblyLocation, path, true);
                        var fileInfo = new FileInfo(path);
                        fileInfo.IsReadOnly = false;
                    }
                    catch (IOException e)
                    {
                        Shell.Instance.Logger.Log(e);
                    }
                }
            }
        }

        public string LocalPath { get; private set; }
    }
}
