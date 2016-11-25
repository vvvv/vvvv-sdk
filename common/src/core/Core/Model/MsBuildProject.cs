using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using VVVV.Core.Logging;
using VVVV.Core.Runtime;
using VVVV.Utils;
using MsBuild = Microsoft.Build;
using System.Text.RegularExpressions;

namespace VVVV.Core.Model
{
    /// <summary>
    /// Base class for all MSBuild based projects, like C#/F#/VB.NET etc.
    /// Uses internal MSBuild project object to load and save.
    /// 
    /// TODO: Handle ProjectReference items in Load/Save
    /// </summary>
    public abstract class MsBuildProject : Project
    {
        private MsBuild.Evaluation.Project FMsProject;
        protected MsBuild.Evaluation.Project MsProject
        {
            get
            {
                if (FMsProject == null)
                {
                    FMsProject = new MsBuild.Evaluation.Project(LocalPath);
                }
                return FMsProject;
            }
        }

        private Guid FGuid;
        public Guid ProjectGuid
        {
            get
            {
                if (FGuid == Guid.Empty)
                {
                    if (File.Exists(LocalPath))
                    {
                        var guid = MsProject.GetPropertyValue("ProjectGuid");
                        if (guid != null)
                            FGuid = new Guid(guid);
                    }
                    
                    if (FGuid == Guid.Empty)
                        FGuid = Guid.NewGuid();
                }
                return FGuid;
            }
        }

        /// <summary>
        /// The full path to the compiled assembly.
        /// </summary>
        public string AssemblyLocation
        {
            get;
            private set;
        }
        
        public MsBuildProject(string path)
            : base(path)
        {
            // Try to find an assembly
            AssemblyLocation = GetExistingAssemblyLocation();
            Load();
        }
        
        protected override void DisposeManaged()
        {
            Unload();
            base.DisposeManaged();
        }

        protected override void OnProjectCompiledSuccessfully(CompilerEventArgs args)
        {
            // Retrieve new assembly location
            AssemblyLocation = args.CompilerResults.PathToAssembly;

            // Copy local references to the output folder
            var results = args.CompilerResults;
            var assemblyDir = Path.GetDirectoryName(results.PathToAssembly);

            foreach (var reference in References.Where((r) => !r.IsGlobal))
            {
                try
                {
                    var srcFileInfo = new FileInfo(reference.AssemblyLocation);
                    var dstFileName = assemblyDir.ConcatPath(Path.GetFileName(srcFileInfo.Name));
                    var dstFileInfo = srcFileInfo.CopyTo(dstFileName, true);
                    dstFileInfo.IsReadOnly = false;
                }
                catch (IOException)
                {
                    // Ignore as file is probably in use (because we loaded it)
                }
            }

            base.OnProjectCompiledSuccessfully(args);
        }
        
        public string AssemblyName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(Name);
            }
        }
        
        public BuildConfiguration BuildConfiguration
        {
            get;
            set;
        }

        public List<string> ReferencePaths
        {
            get;
            protected set;
        }
        
        static readonly char[] FSplitChars = new char[] { ';' };
        private void Load()
        {
            var projectPath = LocalPath;
            var projectDir = Path.GetDirectoryName(projectPath);

            var splitOptions = StringSplitOptions.RemoveEmptyEntries;
            var setupInformation = AppDomain.CurrentDomain.SetupInformation;
            // Always null, why? probing path is set in vvvv.exe.config
            // var searchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            ReferencePaths = new List<string>()
            {
                Path.GetFullPath(Path.Combine(setupInformation.ApplicationBase, "lib", "core")),
                Path.GetFullPath(Path.Combine(setupInformation.ApplicationBase, "lib", "nodes", "plugins"))
            };

            try
            {
                var msBuildProject = MsProject;
                var referencePathProperty = msBuildProject.GetPropertyValue("ReferencePath");
                if (!string.IsNullOrEmpty(referencePathProperty))
                {
                    foreach (var refPath in referencePathProperty.Split(FSplitChars, splitOptions))
                    {
                        var trimmedRefPath = refPath.Trim();
                        trimmedRefPath = refPath.TrimEnd(Path.DirectorySeparatorChar);
                        var absoluteRefPath = Path.IsPathRooted(trimmedRefPath)
                            ? trimmedRefPath
                            : Path.Combine(projectDir, trimmedRefPath);
                        try
                        {
                            absoluteRefPath = Path.GetFullPath(absoluteRefPath);
                            if (!ReferencePaths.Contains(absoluteRefPath) && Directory.Exists(absoluteRefPath))
                            {
                                ReferencePaths.Add(absoluteRefPath);
                            }
                        }
                        catch (NotSupportedException)
                        {
                            // Ignore
                        }
                    }
                }

                // Iterate through the various itemgroups
                // and subsequently through the items
                foreach (var projectItem in msBuildProject.Items)
                {
                    switch (projectItem.ItemType)
                    {
                        case "Reference":
                            IReference reference = null;

                            var include = projectItem.EvaluatedInclude;
                            if (include == "System.ComponentModel.Composition")
                                include = "System.ComponentModel.Composition.Codeplex";

                            if (projectItem.HasMetadata("HintPath"))
                            {
                                var hintPath = projectItem.GetMetadataValue("HintPath");
                                var assemblyLocation = hintPath;
                                if (!Path.IsPathRooted(assemblyLocation))
                                {
                                    assemblyLocation = projectDir.ConcatPath(hintPath);
                                }

                                if (!File.Exists(assemblyLocation))
                                {
                                    //search in reference paths
                                    assemblyLocation = TryAddReferencePath(assemblyLocation, include);
                                }

                                if (File.Exists(assemblyLocation))
                                    assemblyLocation = Path.GetFullPath(assemblyLocation);

                                reference = new AssemblyReference(assemblyLocation);
                            }
                            else
                            {
                                var assemblyLocation = TryAddReferencePath("", include);
                                if (File.Exists(assemblyLocation))
                                    reference = new AssemblyReference(assemblyLocation, true);
                            }


                            // Reference couldn't be found, try GAC
                            if (reference == null)
                            {
                                try
                                {
                                    var assemblyLocation = AssemblyCache.QueryAssemblyInfo(include);
                                    reference = new AssemblyReference(assemblyLocation, true);
                                }
                                catch (Exception)
                                {
                                    reference = new AssemblyReference(string.Format("{0}.dll", include), true);
                                }
                            }

                            if (reference != null)
                                References.Add(reference);
                            break;
                        case "ProjectReference":
                            // TODO: Load project references.
                            break;
                        case "Compile":
                        case "None":
                            IDocument document = null;
                            var canBeCompiled = projectItem.ItemType == "Compile";
                            var documentPath = projectDir.ConcatPath(projectItem.EvaluatedInclude);
                            if (!File.Exists(documentPath))
                                document = new MissingDocument(documentPath, documentPath, canBeCompiled);
                            else
                                FDocumentConverter.Convert(documentPath, out document);
                            if (document != null)
                                Documents.Add(document);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Shell.Instance.Logger.Log(e);
            }
        }

        private void Unload()
        {
            if (FMsProject != null)
            {
                FMsProject.ProjectCollection.UnloadProject(FMsProject);
                FMsProject = null;
            }
        }

        //tries to combine the given path and reference name with the reference paths
        protected string TryAddReferencePath(string path, string referenceName)
        {
            foreach (var refPath in ReferencePaths)
            {
                var pathToTest = string.Format("{0}.dll", refPath.ConcatPath(referenceName));
                if (File.Exists(pathToTest))
                {
                    path = pathToTest;
                    break;
                }
            }

            return path;
        }

        //tests if a reference name is in the reference paths
        private bool InReferencePaths(string refName)
        {
            var testPath = TryAddReferencePath("", refName);
            return testPath != "";
        }

        static MsBuild.Construction.ProjectPropertyElement CreateProperty(MsBuild.Construction.ProjectRootElement project, string name, string value, string condition = null)
        {
            var propertyElement = project.CreatePropertyElement(name);
            propertyElement.Value = value;
            propertyElement.Condition = condition;
            return propertyElement;
        }

        public override void SaveTo(string projectPath)
        {
            var projectDir = Path.GetDirectoryName(projectPath);

            var msBuildProject = MsBuild.Construction.ProjectRootElement.Create();
            try
            {
                msBuildProject.ToolsVersion = "4.0";
                msBuildProject.DefaultTargets = "Build";

                {
                    try
                    {
                        var propertyGroup = msBuildProject.AddPropertyGroup();
                        propertyGroup.AddProperty("ProjectGuid", ProjectGuid.ToString("B").ToUpper());
                        propertyGroup.AppendChild(CreateProperty(msBuildProject, "Configuration", "Debug", " '$(Configuration)' == '' "));
                        propertyGroup.AppendChild(CreateProperty(msBuildProject, "Platform", "x86", " '$(Platform)' == '' "));
                        propertyGroup.AddProperty("OutputType", "Library");
                        propertyGroup.AddProperty("RootNamespace", "VVVV.Nodes");
                        propertyGroup.AddProperty("AssemblyName", AssemblyName);
                        propertyGroup.AddProperty("TargetFrameworkVersion", "v4.0");
                        propertyGroup.AddProperty("OutputPath", @"bin\$(Platform)\$(Configuration)\");
                        propertyGroup.AddProperty("AllowUnsafeBlocks", "True");

                        //add loaded reference paths
                        var referencePaths = ReferencePaths.Select(refPath =>
                            Path.IsPathRooted(refPath)
                                ? PathUtils.MakeRelativePath(projectDir + @"\", refPath + @"\")
                                : refPath);
                        var referencePathValue = string.Join(";", referencePaths);
                        if (!string.IsNullOrEmpty(referencePathValue))
                            propertyGroup.AddProperty("ReferencePath", referencePathValue);
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }

                // From src/Default.Project.settings
                {
                    var propertyGroup = msBuildProject.AddPropertyGroup();
                    propertyGroup.Condition = " '$(Configuration)' == 'Debug' ";
                    propertyGroup.AddProperty("DefineConstants", "DEBUG;TRACE");
                    propertyGroup.AddProperty("Optimize", "False");
                    propertyGroup.AddProperty("CheckForOverflowUnderflow", "True");
                    propertyGroup.AddProperty("DebugType", "Full");
                    propertyGroup.AddProperty("DebugSymbols", "True");
                }

                {
                    var propertyGroup = msBuildProject.AddPropertyGroup();
                    propertyGroup.Condition = " '$(Configuration)' == 'Release' ";
                    propertyGroup.AddProperty("DefineConstants", "TRACE");
                    propertyGroup.AddProperty("Optimize", "True");
                    propertyGroup.AddProperty("CheckForOverflowUnderflow", "False");
                    propertyGroup.AddProperty("DebugType", "None");
                    propertyGroup.AddProperty("DebugSymbols", "False");
                }

                {
                    var propertyGroup = msBuildProject.AddPropertyGroup();
                    propertyGroup.Condition = " '$(Platform)' == 'AnyCPU' ";
                    propertyGroup.AddProperty("PlatformTarget", "AnyCPU");
                }

                {
                    var propertyGroup = msBuildProject.AddPropertyGroup();
                    propertyGroup.Condition = " '$(Platform)' == 'x86' ";
                    propertyGroup.AddProperty("PlatformTarget", "x86");
                }

                {
                    var propertyGroup = msBuildProject.AddPropertyGroup();
                    propertyGroup.Condition = " '$(Platform)' == 'x64' ";
                    propertyGroup.AddProperty("PlatformTarget", "x64");
                }

                //add referenced items
                foreach (var reference in References)
                {
                    var item = msBuildProject.AddItem("Reference", reference.Name);
                    if (!reference.IsGlobal && !InReferencePaths(reference.Name))
                    {
                        var hintPath = reference.GetRelativePath();
                        var dsc = Path.DirectorySeparatorChar;
                        hintPath = hintPath.Replace(dsc + "x86" + dsc, dsc + "$(Platform)");
                        hintPath = hintPath.Replace(dsc + "x64" + dsc, dsc + "$(Platform)");
                        item.AddMetadata("HintPath", hintPath);
                    }
                }

                foreach (var document in Documents)
                    msBuildProject.AddItem(document.CanBeCompiled ? "Compile" : "None", document.GetRelativePath());

                msBuildProject.AddImport("$(MSBuildBinPath)\\Microsoft.CSharp.Targets");

                // Create the project directory if it doesn't exist yet.
                if (!Directory.Exists(projectDir))
                    Directory.CreateDirectory(projectDir);

                msBuildProject.Save(projectPath);
            }
            finally
            {
                MsBuild.Evaluation.ProjectCollection.GlobalProjectCollection.UnloadProject(msBuildProject);
            }

            base.SaveTo(projectPath);
        }

        string GetAssemblyBaseDir() => Path.GetDirectoryName(LocalPath).ConcatPath("bin").ConcatPath("Dynamic");
        string GetAssemblyName() => LocalPath.GetHashCode().ToString();

        protected string GetFreshAssemblyLocation()
        {
            var assemblyBaseDir = GetAssemblyBaseDir();
            var i = 0;
            var name = GetAssemblyName();
            string assemblyLocation = null;
            while (true)
            {
                var assemblyName = string.Format("{0}._dynamic_.{1}.dll", name, ++i);
                assemblyLocation = assemblyBaseDir.ConcatPath(assemblyName);
                if (!File.Exists(assemblyLocation)) break;
            }

            return assemblyLocation;
        }

        public static readonly Regex DynamicRegExp = new Regex(@"(.*)\._dynamic_\.[0-9]+\.dll$");
        string GetExistingAssemblyLocation()
        {
            var assemblyBaseDir = GetAssemblyBaseDir();
            if (Directory.Exists(assemblyBaseDir))
            {
                var dirInfo = new DirectoryInfo(assemblyBaseDir);
                return dirInfo.GetFiles($"{GetAssemblyName()}*.dll")
                    .Where(fi => MsBuildProject.DynamicRegExp.Match(fi.FullName).Success)
                    .OrderBy(fi => fi.LastWriteTime)
                    .Select(fi => fi.FullName)
                    .LastOrDefault();
            }
            return null;
        }

        public static bool IsDynamicAssembly(string filename)
        {
            return DynamicRegExp.IsMatch(filename);
        }
    }
}
