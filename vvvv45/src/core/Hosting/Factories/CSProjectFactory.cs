using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Text;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.PrettyPrinter;
using ICSharpCode.NRefactory.Visitors;
using ICSharpCode.SharpDevelop.Dom;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.Core.Runtime;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
	[Export(typeof(IAddonFactory))]
	public class CSProjectFactory : DotNetPluginFactory
	{
		private Dictionary<string, CSProject> FProjects;
		
		[ImportingConstructor]
		public CSProjectFactory(CompositionContainer parentContainer)
			: base(parentContainer, ".csproj")
		{
			FProjects = new Dictionary<string, CSProject>();
		}
		
		protected override IEnumerable<INodeInfo> GetNodeInfos(string filename)
		{
			var nodeInfos = new List<INodeInfo>();
			
			// Normalize the filename
			filename = new Uri(filename).LocalPath;
			
			CSProject project;
			if (!FProjects.TryGetValue(filename, out project))
			{
				project = new CSProject(new Uri(filename));
				if (FSolution.Projects.CanAdd(project))
				{
					FSolution.Projects.Add(project);
					project.ProjectCompiledSuccessfully += project_ProjectCompiled;
				}
				FProjects[filename] = project;
			}
			
			// Do we need to compile it?
			if (!IsAssemblyUpToDate(project))
			{
				var isLoaded = project.IsLoaded;
				if (!isLoaded)
					project.Load();
				
				project.Compile();
				
				if (!isLoaded)
					project.Unload();
				
				if (project.CompilerResults.Errors.HasErrors)
				{
					var errorLog = GetCompileErrorsLog(project, project.CompilerResults);
					throw new Exception(errorLog);
				}
			}
			
			LoadNodeInfosFromFile(project.AssemblyLocation, filename, ref nodeInfos);
			
			foreach (var nodeInfo in nodeInfos)
			{
				nodeInfo.Type = NodeType.Dynamic;
				nodeInfo.UserData = project;
			}
			
			return nodeInfos;
		}
		
		void project_ProjectCompiled(object sender, CompilerEventArgs args)
		{
			var project = sender as IProject;
			base.FileChanged(project.Location.LocalPath);
		}
		
		private bool IsAssemblyUpToDate(IProject project)
		{
			var now = DateTime.Now;
			var projectTime = File.GetLastWriteTime(project.Location.LocalPath);
			var assemblyTime = File.GetLastWriteTime(project.AssemblyLocation);
			
			// This can happen in case the computer time is wrong or
			// in a different time zone than the project was created in.
			if (now < projectTime)
			{
				projectTime = now - TimeSpan.FromSeconds(10.0);
			}
			
			return File.Exists(project.AssemblyLocation) && (projectTime <= assemblyTime);
		}
		
		protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
		{
			var project = nodeInfo.UserData as IProject;
			if (project != null)
			{
				if (!project.IsLoaded)
					project.Load();
			}
			return base.CreateNode(nodeInfo, pluginHost);
		}

		protected override void DeleteArtefacts(string dir, bool recursive)
		{
			// Dynamic plugins generate a new assembly everytime they are compiled.
			// Cleanup old assemblies.
			var mostRecentFiles = new Dictionary<string, Tuple<string, DateTime>>();
			
			foreach (var file in Directory.GetFiles(dir, "*.dll"))
			{
				try
				{
					var match = FDynamicRegExp.Match(file);
					if (match.Success)
					{
						var fileName = match.Groups[1].Value;
						
						var currentFileTupe = new Tuple<string, DateTime>(file, File.GetLastWriteTime(file));
						if (mostRecentFiles.ContainsKey(fileName))
						{
							// We've seen this file before.
							var mostRecentFileTuple = mostRecentFiles[fileName];
							
							if (currentFileTupe.Item2 > mostRecentFileTuple.Item2)
							{
								// Current file is newer than most recent -> delete most recent and set current as new most recent.
								mostRecentFiles[fileName] = currentFileTupe;
								File.Delete(mostRecentFileTuple.Item1);
								File.Delete(mostRecentFileTuple.Item1.Replace(".dll", ".pdb"));
							}
							else
							{
								// Current file is older than most recent -> delete it.
								File.Delete(currentFileTupe.Item1);
								File.Delete(currentFileTupe.Item1.Replace(".dll", ".pdb"));
							}
						}
						else
							mostRecentFiles.Add(fileName, currentFileTupe);
					}
				}
				catch (Exception e)
				{
					FLogger.Log(e);
				}
			}
			
			base.DeleteArtefacts(dir, recursive);
		}
		
		string GetCompileErrorsLog(IProject project, CompilerResults results)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(string.Format("Compilation of {0} failed. See errors below:\n", project));
			
			foreach (CompilerError error in results.Errors)
			{
				if (!error.IsWarning)
				{
					stringBuilder.Append(string.Format("{0} in {1}:{2}\n", error.ErrorText, error.FileName, error.Line));
				}
			}
			
			return stringBuilder.ToString();
		}
		
		protected override bool CloneNode(INodeInfo nodeInfo, string path, string name, string category, string version, out string newFilename)
		{
			// See if this nodeInfo belongs to us.
			var filename = nodeInfo.Filename;
			if (FProjects.ContainsKey(filename))
			{
				var project = FProjects[filename];
				
				if (!project.IsLoaded)
					project.Load();
				
				string className = name.Replace(" ", "");
				
				// Find a suitable project name
				var newProjectName = className;
				var newProjectPath = path.ConcatPath(newProjectName).ConcatPath(newProjectName + ".csproj");
				
				if (File.Exists(newProjectPath))
				{
					newProjectName = className + category;
					newProjectPath = path.ConcatPath(newProjectName).ConcatPath(newProjectName + ".csproj");
				}
				
				if (File.Exists(newProjectPath))
				{
					newProjectName = className + category + version;
					newProjectPath = path.ConcatPath(newProjectName).ConcatPath(newProjectName + ".csproj");
				}
				
				int i = 1;
				while (File.Exists(newProjectPath))
				{
					newProjectName = className + category + version + i++;
					newProjectPath = path.ConcatPath(newProjectName).ConcatPath(newProjectName + ".csproj");
				}
				
				var newLocation = new Uri(newProjectPath);
				project.SaveTo(newLocation);
				
				var newProject = new CSProject(newLocation);
				newProject.Load();
				
				var newLocationDir = newLocation.GetLocalDir();
				foreach (var doc in newProject.Documents)
				{
					// Now scan the document for possible plugin infos.
					// If we find one, update its properties and rename the class and document.
					if (doc is CSDocument)
					{
						var csDoc = doc as CSDocument;
						
						// Rename the CSDocument
						var docName = Path.GetFileNameWithoutExtension(csDoc.Name);
						if (docName == project.Name)
							csDoc.Name = string.Format("{0}.cs", newProject.Name);
						
						csDoc.WaitParseCompleted();
						
						var parserResults = csDoc.ParserResults;
						var compilationUnit = parserResults.CompilationUnit;
						
						// Write new values to plugin info and remove all other plugin infos.
						var pluginInfoTransformer = new PluginClassTransformer(nodeInfo, name, category, version, className);
						compilationUnit.AcceptVisitor(pluginInfoTransformer, null);
						
						var outputVisitor = new CSharpOutputVisitor();
						var specials = parserResults.Specials;
						
						using (SpecialNodesInserter.Install(specials, outputVisitor))
						{
							outputVisitor.VisitCompilationUnit(compilationUnit, null);
						}
						
						csDoc.TextContent = outputVisitor.Text;
					}
				}
				
				// Save all the documents.
				foreach (var doc in newProject.Documents)
					doc.Save();
				
				// Save the project.
				newProject.Save();
				
				// Unload it.
				newProject.Unload();
				
				// Dispose it.
				newProject.Dispose();
				
				newFilename = newLocation.LocalPath;
				return true;
			}
			
			return base.CloneNode(nodeInfo, path, name, category, version, out newFilename);
		}
	}

	internal class PluginClassTransformer : AbstractAstTransformer
	{
		static string PLUGIN_INFO = typeof(PluginInfoAttribute).Name.Replace("Attribute", "");
		static string NAME = "Name";
		static string CATEGORY = "Category";
		static string VERSION = "Version";
		
		private INodeInfo FNodeInfo;
		private string FName;
		private string FCategory;
		private string FVersion;
		private string FClassName;
		
		/// <summary>
		/// Sets the Name, Category and Version properties of a PluginInfoAttribute matching
		/// the given nodeInfo to the new name, category and version.
		/// Deletes all other occurences of PluginInfoAttribute.
		/// Updates the name of the class attributed with found PluginInfoAttribute.
		/// </summary>
		/// <param name="nodeInfo">The node info to replace.</param>
		/// <param name="name">The new name.</param>
		/// <param name="category">The new category.</param>
		/// <param name="version">The new version.</param>
		public PluginClassTransformer(INodeInfo nodeInfo, string name, string category, string version, string className)
		{
			Debug.Assert(!string.IsNullOrEmpty(name));
			Debug.Assert(!string.IsNullOrEmpty(category));
			
			FNodeInfo = nodeInfo;
			FName = name;
			FCategory = category;
			FVersion = version;
			FClassName = className;
		}
		
		public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
		{
			var attributeSectionsToRemove = new List<ICSharpCode.NRefactory.Ast.AttributeSection>();
			bool foundPluginInfo = false;
			
			foreach (var attributeSection in typeDeclaration.Attributes)
			{
				var attributesToRemove = new List<ICSharpCode.NRefactory.Ast.Attribute>();
				
				foreach (var attribute in attributeSection.Attributes)
				{
					if (attribute.Name == PLUGIN_INFO)
					{
						var pluginInfo = ConvertAttributeToPluginInfo(attribute);
						
						if (pluginInfo.Systemname == FNodeInfo.Systemname)
						{
							UpdatePluginInfoAttribute(attribute);
							foundPluginInfo = true;
						}
						else
							attributesToRemove.Add(attribute);
					}
				}
				
				foreach (var attribute in attributesToRemove)
					attributeSection.Attributes.Remove(attribute);
				
				
				if (attributeSection.Attributes.Count == 0)
					attributeSectionsToRemove.Add(attributeSection);
			}
			
			foreach (var attributeSection in attributeSectionsToRemove)
				typeDeclaration.Attributes.Remove(attributeSection);
			
			if (foundPluginInfo)
			{
				typeDeclaration.Name = FClassName;
			}
			
			return base.VisitTypeDeclaration(typeDeclaration, data);
		}
		
		private PluginInfoAttribute ConvertAttributeToPluginInfo(ICSharpCode.NRefactory.Ast.Attribute attribute)
		{
			Debug.Assert(attribute.Name == PLUGIN_INFO);
			
			var pluginInfo = new PluginInfoAttribute();
			
			foreach (var argument in attribute.NamedArguments)
			{
				var expression = argument.Expression as PrimitiveExpression;
				if (expression != null)
				{
					if (argument.Name == NAME)
						pluginInfo.Name = expression.Value as string;
					else if (argument.Name == CATEGORY)
						pluginInfo.Category = expression.Value as string;
					else if (argument.Name == VERSION)
						pluginInfo.Version = expression.Value as string;
				}
			}
			
			return pluginInfo;
		}
		
		private void UpdatePluginInfoAttribute(ICSharpCode.NRefactory.Ast.Attribute attribute)
		{
			Debug.Assert(attribute.Name == PLUGIN_INFO);
			
			var namedArguments = attribute.NamedArguments;
			
			var oldArguments = new List<NamedArgumentExpression>();
			foreach (var argument in namedArguments)
				if (argument.Name == NAME || argument.Name == CATEGORY || argument.Name == VERSION)
					oldArguments.Add(argument);
			
			foreach (var argument in oldArguments)
				namedArguments.Remove(argument);
			
			namedArguments.Insert(0, new NamedArgumentExpression(NAME, new PrimitiveExpression(FName, FName)));
			namedArguments.Insert(1, new NamedArgumentExpression(CATEGORY, new PrimitiveExpression(FCategory, FCategory)));
			if (!string.IsNullOrEmpty(FVersion))
				namedArguments.Insert(2, new NamedArgumentExpression(VERSION, new PrimitiveExpression(FVersion, FVersion)));
		}
	}
}
