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
                    FMsProject = new MsBuild.Evaluation.Project(Location.LocalPath);
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
					if (File.Exists(Location.LocalPath))
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
		
		public MsBuildProject(string name, Uri location)
			:base(name, location)
		{
			// Try to find an assembly
			var assemblyBaseDir = Path.GetDirectoryName(AssemblyLocation);
			if (Directory.Exists(assemblyBaseDir))
			{
				foreach (var file in Directory.GetFiles(assemblyBaseDir, "*.dll"))
				{
					AssemblyLocation = file;
					break;
				}
			}
			
			ProjectCompiledSuccessfully += this_ProjectCompiledSuccessfully;
		}
		
		protected override void DisposeManaged()
		{
			ProjectCompiledSuccessfully -= this_ProjectCompiledSuccessfully;
			base.DisposeManaged();
		}

		void this_ProjectCompiledSuccessfully(object sender, CompilerEventArgs args)
		{
			// Copy local references
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
				} catch (IOException)
				{
					// Ignore as file is probably in use (because we loaded it)
				}
			}
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
		protected override void DoLoad()
		{
			var projectPath = Location.LocalPath;
			var projectDir = Path.GetDirectoryName(projectPath);

            var msBuildProject = MsProject;
			var splitOptions = StringSplitOptions.RemoveEmptyEntries;
			var setupInformation = AppDomain.CurrentDomain.SetupInformation;
			// Always null, why? probing path is set in vvvv.exe.config
			// var searchPath = AppDomain.CurrentDomain.RelativeSearchPath;
			ReferencePaths = new List<string>()
			{
				Path.GetFullPath(Path.Combine(setupInformation.ApplicationBase, "lib", "core")),
                Path.GetFullPath(Path.Combine(setupInformation.ApplicationBase, "lib", "nodes", "plugins"))
			};
			
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
                    absoluteRefPath = Path.GetFullPath(absoluteRefPath);
                    if (!ReferencePaths.Contains(absoluteRefPath))
					{
                        ReferencePaths.Add(absoluteRefPath);
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
                        IDocument document;
                        if (FDocumentConverter.Convert(projectDir.ConcatPath(projectItem.EvaluatedInclude), out document))
                        {
                            Documents.Add(document);
                            document.Load();
                        }
                        break;
                    default:
                        break;
                }
            }
			
			base.DoLoad();
		}

        protected override void DoUnload()
        {
            if (FMsProject != null)
            {
                FMsProject.ProjectCollection.UnloadProject(FMsProject);
                FMsProject = null;
            }
            base.DoUnload();
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
		
		public override void SaveTo(Uri location)
		{
			var projectPath = location.LocalPath;
			var projectDir = Path.GetDirectoryName(projectPath);

            var msBuildProject = MsBuild.Construction.ProjectRootElement.Create();
            msBuildProject.ToolsVersion = "4.0";
            msBuildProject.DefaultTargets = "Build";

            var propertyGroup = msBuildProject.AddPropertyGroup();
            propertyGroup.AddProperty("ProjectGuid", ProjectGuid.ToString("B").ToUpper());
            propertyGroup.AddProperty("Configuration", "Debug");
            propertyGroup.AddProperty("Platform", "x86");
            propertyGroup.AddProperty("OutputType", "Library");
            propertyGroup.AddProperty("RootNamespace", "VVVV.Nodes");
            propertyGroup.AddProperty("AssemblyName", AssemblyName);
            propertyGroup.AddProperty("TargetFrameworkVersion", "v4.0");
            propertyGroup.AddProperty("OutputPath", "bin\\Debug\\");
            propertyGroup.AddProperty("DebugSymbols", "True");
            propertyGroup.AddProperty("DebugType", "Full");
            propertyGroup.AddProperty("Optimize", "False");
            propertyGroup.AddProperty("CheckForOverflowUnderflow", "True");
            propertyGroup.AddProperty("DefineConstants", "DEBUG;TRACE");
            propertyGroup.AddProperty("AllowUnsafeBlocks", "True");

            //add loaded reference paths
            var referencePaths = ReferencePaths.Select(refPath =>
                Path.IsPathRooted(refPath)
                    ? PathUtils.MakeRelativePath(projectDir + @"\", refPath + @"\")
                    : refPath);
            var referencePathValue = string.Join(";", referencePaths);
            if (!string.IsNullOrEmpty(referencePathValue))
                propertyGroup.AddProperty("ReferencePath", referencePathValue);

            msBuildProject.AddImport("$(MSBuildBinPath)\\Microsoft.CSharp.Targets");

            //add reference items
            foreach (var reference in References)
            {
                var item = msBuildProject.AddItem("Reference", reference.Name);
                if (!reference.IsGlobal && !InReferencePaths(reference.Name))
                {
                    var hintPath = reference.GetRelativePath();
                    item.AddMetadata("HintPath", hintPath);
                }
            }

            foreach (var document in Documents)
                msBuildProject.AddItem(document.CanBeCompiled ? "Compile" : "None", document.GetRelativePath());

            // Create the project directory if it doesn't exist yet.
            if (!Directory.Exists(projectDir))
                Directory.CreateDirectory(projectDir);

            msBuildProject.Save(projectPath);
			
			base.SaveTo(location);
		}
	}
}
