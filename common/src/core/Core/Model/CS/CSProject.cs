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
        public CSProject(string name, Uri location)
            : base(name, location)
        {
            FProjectContent = new DefaultProjectContent();
            FProjectContent.Language = LanguageProperties.CSharp;
            Loaded += MsBuildProject_Loaded;
        }
        
        void References_Removed(IViewableCollection<IReference> collection, IReference item)
        {
            FIsProjectContentValid = false;
        }

        void References_Added(IViewableCollection<IReference> collection, IReference item)
        {
            FIsProjectContentValid = false;
        }
        
        void MsBuildProject_Loaded(object sender, EventArgs e)
        {
            FIsProjectContentValid = false;
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
            var solution = Solution as Solution;
            if (solution == null)
            {
                Shell.Instance.Logger.Log(LogType.Warning, "Can't setup project content because Solution property either not set or not of type Solution.");
                return;
            }

            try
            {
                var pcRegistry = ((Solution)this.Solution).ProjectContentRegistry;

                // Clear all referenced contents
                lock (FProjectContent.ReferencedContents)
                {
                    FProjectContent.ReferencedContents.Clear();
                }

                // Add mscorlib
                FProjectContent.AddReferencedContent(pcRegistry.Mscorlib);

                // Add referenced contents
                foreach (var reference in References)
                {
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
        }
        
        private readonly DefaultProjectContent FProjectContent;
        private bool FIsProjectContentValid;
        public IProjectContent ProjectContent
        {
            get
            {
                if (!FIsProjectContentValid)
                {
                    FIsProjectContentValid = true;
                    ReloadProjectContent();
                }
                return FProjectContent;
            }
        }
        
        protected override void DoLoad()
        {
            base.DoLoad();
            References.Added += References_Added;
            References.Removed += References_Removed;
        }
        
        protected override void DoUnload()
        {
            References.Added -= References_Added;
            References.Removed -= References_Removed;
            base.DoUnload();
        }
        
        protected override CompilerResults DoCompile()
        {
            return CSCompiler.Instance.Compile(this);
        }
    }
}
