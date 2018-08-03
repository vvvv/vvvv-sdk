using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

using ICSharpCode.SharpDevelop.Dom;
using VVVV.Core.Logging;
using Microsoft.CSharp;
using System.IO;
using NuGetAssemblyLoader;

namespace VVVV.Core.Model.CS
{
    // TODO: Parsing of references and documents is not complete. Missing events (doc added/removed, ref added/removed..)
    public class CSProject : MsBuildProject
    {
        private static CSharpCodeProvider FProvider;

        static CSProject()
        {
            var compilersPackage = AssemblyLoader.FindPackageAndCacheResult("Microsoft.Net.Compilers");
            var path = compilersPackage != null ? Path.Combine(AssemblyLoader.GetPathOfPackage(compilersPackage), "tools") : "";
            
            if (Directory.Exists(path))
            {
                Environment.SetEnvironmentVariable("ROSLYN_COMPILER_LOCATION", path);
                FProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
            }
            else
            {
                var options = new Dictionary<string, string>();
                options.Add("CompilerVersion", "v4.0");
                FProvider = new CSharpCodeProvider(options);
            }
        }

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
                    if (reference is MsBuildProjectReference)
                    {
                        var projectReference = reference as MsBuildProjectReference;
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
            var files =
                from doc in Documents
                where doc is CSDocument
                select doc.LocalPath;

            var assemblyLocation = GetFreshAssemblyLocation();
            var assemblyBaseDir = Path.GetDirectoryName(assemblyLocation);

            if (!Directory.Exists(assemblyBaseDir))
                Directory.CreateDirectory(assemblyBaseDir);

            var compilerParams = new CompilerParameters();
            compilerParams.WarningLevel = 4;
            compilerParams.OutputAssembly = assemblyLocation;
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = false;
            compilerParams.IncludeDebugInformation = true;
            compilerParams.CompilerOptions += "/unsafe ";

            switch (BuildConfiguration)
            {
                case BuildConfiguration.Release:
                    compilerParams.CompilerOptions += "/optimize ";
                    break;
                case BuildConfiguration.Debug:
                    break;
            }

            foreach (var reference in References)
            {
                var location = reference.AssemblyLocation;
                if (Path.GetExtension(location) != ".dll")
                    location = string.Format("{0}.dll", location);

                compilerParams.ReferencedAssemblies.Add(location);
            }

            return FProvider.CompileAssemblyFromFile(compilerParams, files.ToArray());
        }
    }
}
