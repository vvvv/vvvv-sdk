using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using ICSharpCode.SharpDevelop.Dom;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Runtime;
using VVVV.Core.Runtime.CS;

namespace VVVV.Core.Model.CS
{
    // TODO: Parsing of references and documents is not complete. Missing events (doc added/removed, ref added/removed..)
    public class CSProject : MsBuildProject
    {
        private readonly ManualResetEvent FPCLoadingIsDone = new ManualResetEvent(true);
        private volatile bool FCancelLoading = false;

        public CSProject(string name, Uri location)
            : base(name, location)
        {
            FProjectContent = new DefaultProjectContent();
            FProjectContent.Language = LanguageProperties.CSharp;
            Loaded += MsBuildProject_Loaded;
        }

        void References_Removed(IViewableCollection<IReference> collection, IReference item)
        {
            ReloadProjectContent();
        }

        void References_Added(IViewableCollection<IReference> collection, IReference item)
        {
            ReloadProjectContent();
        }

        void MsBuildProject_Loaded(object sender, EventArgs e)
        {
            ReloadProjectContent();
        }

        protected override void DisposeManaged()
        {
            Loaded -= MsBuildProject_Loaded;
            if (IsLoaded)
                Unload();
            base.DisposeManaged();
        }

        private void ReloadProjectContent()
        {
            // Setup project content in background (might take long)
            if (Solution != null && Solution is Solution)
            {
                FPCLoadingIsDone.Reset();
                ThreadPool.QueueUserWorkItem(SetupProjectContent);
            }
            else
                Shell.Instance.Logger.Log(LogType.Warning, "Can't setup project content because Solution property either not set or not of type Solution.");
        }

        private DefaultProjectContent FProjectContent;
        public IProjectContent ProjectContent
        {
            get
            {
                return FProjectContent;
            }
        }

        protected override void DoLoad()
        {
            FCancelLoading = false;

            base.DoLoad();
            References.Added += References_Added;
            References.Removed += References_Removed;
        }

        protected override void DoUnload()
        {
            References.Added -= References_Added;
            References.Removed -= References_Removed;

            FCancelLoading = true;

            FPCLoadingIsDone.WaitOne();
            base.DoUnload();
        }

        void SetupProjectContent(object state)
        {
            try
            {
                var pcRegistry = ((Solution)this.Solution).ProjectContentRegistry;

                // Clear all referenced contents
                lock (FProjectContent.ReferencedContents)
                {
                    FProjectContent.ReferencedContents.Clear();
                }

                if (FCancelLoading) return;

                // Add mscorlib
                FProjectContent.AddReferencedContent(pcRegistry.Mscorlib);

                // Add referenced contents
                foreach (var reference in References)
                {
                    if (FCancelLoading) return;

                    if (reference is ProjectReference)
                    {
                        var projectReference = reference as ProjectReference;
                        if (projectReference.ReferencedProject is CSProject)
                        {
                            var referencePC = ((CSProject)projectReference.ReferencedProject).ProjectContent;
                            FProjectContent.AddReferencedContent(referencePC);
                        }
                    }
                    else if (reference is IReference)
                    {
                        var assemblyName = reference.Name;
                        var assemblyFilename = reference.AssemblyLocation;
                        var referencePC = pcRegistry.GetProjectContentForReference(assemblyName, assemblyFilename);
                        FProjectContent.AddReferencedContent(referencePC);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore
            }
            finally
            {
                FPCLoadingIsDone.Set();
            }
        }

        protected override CompilerResults DoCompile()
        {
            return CSCompiler.Instance.Compile(this);
        }
    }
}
