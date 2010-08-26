using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.PrettyPrinter;
using ICSharpCode.NRefactory.Visitors;
using ICSharpCode.SharpDevelop.Dom;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
	[Export(typeof(IAddonFactory))]
	public class CSProjectFactory : AbstractFileFactory
	{
		[Import]
		protected DotNetPluginFactory FPluginFactory;
		
		[Import]
		protected ISolution FSolution;
		
		[Import]
		protected ILogger FLogger;
		
		private Dictionary<string, CSProject> FProjects;
		
		public CSProjectFactory()
		{
			FFileExtension = ".csproj";
			FDirectory = Path.Combine(FDirectory, @"..\..\dynamic");
			
			FProjects = new Dictionary<string, CSProject>();
		}
		
		public override IEnumerable<INodeInfo> ExtractNodeInfos(string filename)
		{
			if (Path.GetExtension(filename) != FFileExtension) yield break;
			
			// Normalize the filename
			filename = new Uri(filename).LocalPath;
			
			CSProject project;
			
			if (!FProjects.TryGetValue(filename, out project))
			{
				project = new CSProject(new Uri(filename));
				if (FSolution.Projects.CanAdd(project))
					FSolution.Projects.Add(project);
				project.Load();
				project.CompileCompleted += new CompileCompletedHandler(project_CompileCompleted);
				project.ProjectCompiled += project_ProjectCompiled;
				FProjects[filename] = project;
				
				project.Compile();
			}
			else
				project.CompileAsync();
			
			yield break;
		}

		void project_CompileCompleted(IProject project, CompilerResults results)
		{
			if (results.Errors.HasErrors)
			{
				FLogger.Log(LogType.Warning, "Compilation of {0} failed. See errors below:", project);
				
				foreach (CompilerError error in results.Errors)
				{
					if (!error.IsWarning)
					{
						FLogger.Log(LogType.Warning, "{0} in {1}:{2}", error.ErrorText, error.FileName, error.Line);
					}
				}
			}
		}

		void project_ProjectCompiled(IProject project, IExecutable executable)
		{
			FPluginFactory.Register(executable);
		}
		
		#region IAddonFactory
		
		public override bool Clone(INodeInfo nodeInfo, string path, string name, string category, string version)
		{
			string className = name.Replace(" ", "");
			
			// See if this nodeInfo belongs to us.
			var filename = nodeInfo.Filename;
			if (FProjects.ContainsKey(filename))
			{
				var project = FProjects[filename];
				
				// Create a new project by generating a new name first.
				var solutionDir = FSolution.Location.GetLocalDir();
				
				var newProjectName = className;
				var newProjectPath = solutionDir.ConcatPath(newProjectName).ConcatPath(newProjectName + ".csproj");
				var newLocation = new Uri(newProjectPath);
				var newProject = project.Clone() as CSProject;
				
				// Move the cloned project and all its documents to the new location.
				newProject.Location = newLocation;
				
				var newLocationDir = newLocation.GetLocalDir();
				foreach (var doc in newProject.Documents)
				{
					// The documents are cloned but their location still refers to the old one.
					var relativeDir = doc.GetRelativePath(project);
					newLocation = new Uri(newLocationDir.ConcatPath(relativeDir));
					doc.Location = newLocation;
					
					// Now scan the document for possible plugin infos.
					// If we find one, update its properties and rename the class and document.
					if (doc is CSDocument)
					{
						var csDoc = doc as CSDocument;
						
						// Rename the CSDocument
						var docName = Path.GetFileNameWithoutExtension(csDoc.Name);
						if (docName == project.Name)
							csDoc.Name = string.Format("{0}.cs", newProject.Name);
						
						// We need to parse the method bodies.
						csDoc.Parse(true);
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
				
				return true;
			}
			
			return base.Clone(nodeInfo, path, name, category, version);
		}
		
		#endregion
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
