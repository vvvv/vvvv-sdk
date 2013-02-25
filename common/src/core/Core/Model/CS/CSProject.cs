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
        public CSProject(string path)
            : base(path)
        {
            FProjectContent = new DefaultProjectContent();
            FProjectContent.Language = LanguageProperties.CSharp;
            References.Added += References_Changed;
            References.Removed += References_Changed;
        }

        protected override void DisposeManaged()
        {
            References.Added -= References_Changed;
            References.Removed -= References_Changed;
            base.DisposeManaged();
        }
        
        void References_Changed(IViewableCollection<IReference> collection, IReference item)
        {
            FIsProjectContentValid = false;
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
        
        protected override CompilerResults DoCompile()
        {
            return CSCompiler.Instance.Compile(this);
        }
    }
}
