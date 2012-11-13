using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using VVVV.Core.Logging;
using VVVV.Core.Runtime;
using VVVV.Utils;
using MsBuild = Microsoft.Build.BuildEngine;

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
		private Guid FGuid;
		public Guid ProjectGuid
		{
			get
			{
				if (FGuid == Guid.Empty)
				{
					if (File.Exists(Location.LocalPath))
					{
						var msBuildProject = new MsBuild.Project();
						msBuildProject.Load(Location.LocalPath);
						
						var guid = msBuildProject.GetEvaluatedProperty("ProjectGuid");
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
		
		protected override void DoLoad()
		{
			var projectPath = Location.LocalPath;
			var projectDir = Path.GetDirectoryName(projectPath);
			
			var msBuildProject = new MsBuild.Project();
			msBuildProject.Load(projectPath);

			var splitChars = new char[] { ';' };
			var splitOptions = StringSplitOptions.RemoveEmptyEntries;
			var setupInformation = AppDomain.CurrentDomain.SetupInformation;
			// Always null, why? probing path is set in vvvv.exe.config
			// var searchPath = AppDomain.CurrentDomain.RelativeSearchPath;
			ReferencePaths = new List<string>()
			{
				Path.Combine(setupInformation.ApplicationBase, "lib", "core")
			};
			
			var referencePathProperty = msBuildProject.GetEvaluatedProperty("ReferencePath");
			if (!string.IsNullOrEmpty(referencePathProperty))
			{
				foreach (var refPath in referencePathProperty.Split(splitChars, splitOptions))
				{
					if (!ReferencePaths.Contains(refPath.Trim()))
					{
						ReferencePaths.Add(refPath);
					}
				}
			}
			
			// Iterate through the various itemgroups
			// and subsequently through the items
			foreach (MsBuild.BuildItemGroup itemGroup in msBuildProject.ItemGroups)
			{
				foreach (MsBuild.BuildItem item in itemGroup)
				{
					switch (item.Name)
					{
						case "Reference":
							IReference reference = null;
							
							if (item.Include == "System.ComponentModel.Composition")
								item.Include = "System.ComponentModel.Composition.Codeplex";
							
							if (item.HasMetadata("HintPath"))
							{
								var hintPath = item.GetEvaluatedMetadata("HintPath");
								var assemblyLocation = hintPath;
								if (!Path.IsPathRooted(assemblyLocation))
								{
									assemblyLocation = projectDir.ConcatPath(hintPath);
								}
								
								if (!File.Exists(assemblyLocation))
								{
									//search in reference paths
									assemblyLocation = TryAddReferencePath(assemblyLocation, item.Include);
								}
								
								if (File.Exists(assemblyLocation))
									assemblyLocation = Path.GetFullPath(assemblyLocation);
								
								reference = new AssemblyReference(assemblyLocation);
							}
							else
							{
								var assemblyLocation = TryAddReferencePath("", item.Include);
								if (File.Exists(assemblyLocation))
									reference = new AssemblyReference(assemblyLocation, true);
							}
							
							
							// Reference couldn't be found, try GAC
							if (reference == null)
							{
								try
								{
									var assemblyLocation = AssemblyCache.QueryAssemblyInfo(item.Include);
									reference = new AssemblyReference(assemblyLocation, true);
								}
								catch (Exception)
								{
									reference = new AssemblyReference(string.Format("{0}.dll", item.Include), true);
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
							if (FDocumentConverter.Convert(projectDir.ConcatPath(item.Include), out document))
							{
								Documents.Add(document);
								document.Load();
							}
							break;
						default:
							break;
					}
				}
			}
			
			base.DoLoad();
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
			
			var msBuildProject = new MsBuild.Project();
			msBuildProject.DefaultToolsVersion = "4.0";
			msBuildProject.DefaultTargets = "Build";
			
			var propertyGroup = msBuildProject.AddNewPropertyGroup(false);
			propertyGroup.AddNewProperty("ProjectGuid", ProjectGuid.ToString("B").ToUpper());
			propertyGroup.AddNewProperty("Configuration", "Debug");
			propertyGroup.AddNewProperty("Platform", "x86");
			propertyGroup.AddNewProperty("OutputType", "Library");
			propertyGroup.AddNewProperty("RootNamespace", "VVVV.Nodes");
			propertyGroup.AddNewProperty("AssemblyName", AssemblyName);
			propertyGroup.AddNewProperty("TargetFrameworkVersion", "v4.0");
			propertyGroup.AddNewProperty("OutputPath", "bin\\Debug\\");
			propertyGroup.AddNewProperty("DebugSymbols", "True");
			propertyGroup.AddNewProperty("DebugType", "Full");
			propertyGroup.AddNewProperty("Optimize", "False");
			propertyGroup.AddNewProperty("CheckForOverflowUnderflow", "True");
			propertyGroup.AddNewProperty("DefineConstants", "DEBUG;TRACE");
			propertyGroup.AddNewProperty("AllowUnsafeBlocks", "True");

			//add loaded reference paths
			var expandedVVVV45Path =  msBuildProject.GetEvaluatedProperty("ReferencePath");
			foreach (var refPath in ReferencePaths)
			{
				if (refPath != expandedVVVV45Path)
				{
					if (Path.IsPathRooted(refPath))
					{
						propertyGroup.AddNewProperty("ReferencePath", PathUtils.MakeRelativePath(projectDir + @"\", refPath + @"\"));
					}
					else
					{
						propertyGroup.AddNewProperty("ReferencePath", refPath);
					}
				}
			}
			
			msBuildProject.AddNewImport("$(MSBuildBinPath)\\Microsoft.CSharp.Targets", null);
			
			//add reference items
			foreach (var reference in References)
			{
				var item = msBuildProject.AddNewItem("Reference", reference.Name);
				if (!reference.IsGlobal && !InReferencePaths(reference.Name))
				{
					var hintPath = reference.GetRelativePath();
					item.SetMetadata("HintPath", hintPath);
				}
			}
			
			foreach (var document in Documents)
				msBuildProject.AddNewItem(document.CanBeCompiled ? "Compile" : "None", document.GetRelativePath());
			
			// Create the project directory if it doesn't exist yet.
			if (!Directory.Exists(projectDir))
				Directory.CreateDirectory(projectDir);
			
			msBuildProject.Save(projectPath);
			
			base.SaveTo(location);
		}
	}
}
