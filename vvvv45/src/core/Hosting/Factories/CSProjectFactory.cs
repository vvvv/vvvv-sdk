using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
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
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Linq;

namespace VVVV.Hosting.Factories
{
    [Export(typeof(IAddonFactory))]
    [Export(typeof(CSProjectFactory))]
    [ComVisible(false)]
    public class CSProjectFactory : DotNetPluginFactory
    {
        private readonly Version FPluginInterfacesVersion = typeof(IPluginBase).Assembly.GetName().Version;
        
        [Import]
        protected ISolution FSolution;
        
        [ImportingConstructor]
        public CSProjectFactory(CompositionContainer parentContainer, INodeInfoFactory nodeInfoFactory)
            : base(parentContainer, ".csproj")
        {
            // Listen to stuff added by nodelist.xml
            nodeInfoFactory.NodeInfoAdded += HandleNodeInfoAdded;
        }

        void HandleNodeInfoAdded(object sender, INodeInfo nodeInfo)
        {
            if (nodeInfo.Type == NodeType.Dynamic && nodeInfo.Factory == this)
            {
                nodeInfo.UserData = CreateProject(nodeInfo.Filename);
            }
        }
        
        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            var nodeInfos = new List<INodeInfo>();
            // Normalize the filename
            filename = new Uri(filename).LocalPath;
            var project = CreateProject(filename);
            try
            {
                // Do we need to compile it?
                RecompileIfNeeded(project);

                LoadNodeInfosFromFile(project.AssemblyLocation, filename, ref nodeInfos, false);
            }
            finally
            {
                foreach (var nodeInfo in nodeInfos)
                {
                    nodeInfo.Type = NodeType.Dynamic;
                    nodeInfo.UserData = project;
                    nodeInfo.CommitUpdate();
                }
            }
            return nodeInfos;
        }
        
        private CSProject CreateProject(string filename)
        {
            var project = FSolution.Projects[filename] as CSProject;
            if (project == null)
            {
                var binDir = Path.GetDirectoryName(filename).ConcatPath("bin").ConcatPath("Dynamic");
                DeleteArtefacts(binDir);

                project = new CSProject(filename);
                FSolution.Projects.Add(project);
                project.ProjectCompiledSuccessfully += project_ProjectCompiled;
                project.CompileCompleted += project_CompileCompleted;
            }
            return project;
        }

        void project_CompileCompleted(object sender, CompilerEventArgs args)
        {
            bool hasErrors = false;
            
            var compilerResults = args.CompilerResults;
            if (compilerResults != null)
            {
                var errors = compilerResults.Errors ?? new CompilerErrorCollection();
                hasErrors = errors.HasErrors;
            }
            
            var affectedNodes =
                from node in FHDEHost.RootNode.AsDepthFirstEnumerable()
                where sender == node.NodeInfo.UserData
                select node;
            
            foreach (var node in affectedNodes)
            {
                if (hasErrors)
                    node.Status |= StatusCode.HasInvalidData;
                else
                    node.Status &= ~StatusCode.HasInvalidData;
            }
        }
        
        private bool RecompileIfNeeded(CSProject project)
        {
            if (!FHDEHost.IsBlackBoxMode && !IsAssemblyUpToDate(project))
            {
                FLogger.Log(LogType.Message, "Assembly of {0} is not up to date. Need to recompile ...", project.Name);
                
                project.ProjectCompiledSuccessfully -= project_ProjectCompiled;
                project.Compile();
                project.ProjectCompiledSuccessfully += project_ProjectCompiled;
                
                if (project.CompilerResults.Errors.HasErrors)
                {
                    FLogger.Log(LogType.Error, GetCompileErrorsLog(project, project.CompilerResults));
                    return false;
                }
            }
            return true;
        }
        
        protected override void DoAddFile(string filename)
        {
            CreateProject(filename);
            base.DoAddFile(filename);
        }
        
        protected override void DoRemoveFile(string filename)
        {
            var project = FSolution.Projects[filename];
            if (project != null)
            {
                FSolution.Projects.Remove(project);
                project.ProjectCompiledSuccessfully -= project_ProjectCompiled;
                project.CompileCompleted -= project_CompileCompleted;
                project.Dispose();
            }
            
            base.DoRemoveFile(filename);
        }
        
        void project_ProjectCompiled(object sender, CompilerEventArgs args)
        {
            var project = sender as IProject;
            base.FileChanged(project.LocalPath);
        }
        
        private bool IsAssemblyUpToDate(CSProject project)
        {
            var assemblyLocation = project.AssemblyLocation;
            if (assemblyLocation == null) return false;
            if (!File.Exists(assemblyLocation)) return false;

            var now = DateTime.Now;
            var projectTime = new [] { File.GetLastWriteTime(project.LocalPath) }
                .Concat(project.Documents.Select(d => File.GetLastWriteTime(d.LocalPath)))
                .Max();
            var assemblyTime = File.GetLastWriteTime(assemblyLocation);
            
            // This can happen in case the computer time is wrong or
            // in a different time zone than the project was created in.
            if (now < projectTime)
            {
                projectTime = now - TimeSpan.FromSeconds(10.0);
            }
            
            if (projectTime <= assemblyTime)
            {
                // We also need to check if the version info of the referenced assemblies
                // is the same as the one referenced by the project file.
                // We only check the PluginInterfaces assembly here to save performance.
                try
                {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyLocation);
                    var piAssembly = assembly.GetReferencedAssemblies().Where(assemblyName => assemblyName.Name == typeof(IPluginBase).Assembly.GetName().Name).FirstOrDefault();

                    if (piAssembly != null && piAssembly.Version != FPluginInterfacesVersion)
                        return false;

                    switch (project.BuildConfiguration)
                    {
                        case BuildConfiguration.Release:
                            return !IsAssemblyDebugBuild(assembly);
                        case BuildConfiguration.Debug:
                            return IsAssemblyDebugBuild(assembly);
                        default:
                            return true;
                    }
                }
                catch (Exception e)
                {
                    // Log the exception and return true
                    FLogger.Log(e);
                    return true;
                }
            }
            
            return false;
        }

        private bool IsAssemblyDebugBuild(Assembly assembly)
        {
            foreach (var attribute in assembly.GetCustomAttributesData())
            {
                if (attribute.ToString().Contains(typeof(DebuggableAttribute).FullName))
                {
                    var debuggingModes = (DebuggableAttribute.DebuggingModes)attribute.ConstructorArguments.First().Value;
                    return (debuggingModes & DebuggableAttribute.DebuggingModes.DisableOptimizations) > 0;
                }
            }
            return false;
        }
        
        protected void DeleteArtefacts(string dir)
        {
            // Nothing to do if not existent.
            if (FHDEHost.IsBlackBoxMode || !Directory.Exists(dir)) return;
            
            // Dynamic plugins generate a new assembly everytime they are compiled.
            // Cleanup old assemblies.
            var mostRecentFiles = new Dictionary<string, Tuple<string, DateTime>>();
            
            foreach (var file in Directory.GetFiles(dir, "*.dll"))
            {
                try
                {
                    var match = MsBuildProject.DynamicRegExp.Match(file);
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
        }
        
        private static string GetCompileErrorsLog(IProject project, CompilerResults results)
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
        
        protected override bool GetAssemblyLocation (INodeInfo nodeInfo, out string assemblyLocation)
        {
            var project = CreateProject(nodeInfo.Filename);
            var isUpToData = RecompileIfNeeded(project);
            assemblyLocation = project.AssemblyLocation;
            return isUpToData;
        }
        
        protected override bool CloneNode(INodeInfo nodeInfo, string path, string name, string category, string version, out string newFilename)
        {
            string className = string.Format("{0}{1}{2}Node", version, category, name);
            className = Regex.Replace(className, @"[^a-zA-Z0-9]+", "_");
            var regexp = new Regex(@"^[0-9]+");
            if (regexp.IsMatch(className))
                className = string.Format("C{0}", className);
            
            // Find a suitable project name
            var newProjectName = string.Format("{0}{1}{2}", version, category, name);;
            var newProjectPath = path.ConcatPath(newProjectName).ConcatPath(newProjectName + ".csproj");
            
            int i = 1;
            string tmpNewProjectName = newProjectName;
            string tmpClassName = className;
            while (File.Exists(newProjectPath))
            {
                newProjectName = tmpNewProjectName + i;
                className = tmpClassName + i++;
                newProjectPath = path.ConcatPath(newProjectName).ConcatPath(newProjectName + ".csproj");
            }

            var filename = nodeInfo.Filename;
            var project = CreateProject(filename);
            project.SaveTo(newProjectPath);

            using (var newProject = new CSProject(newProjectPath))
            {
                foreach (var doc in newProject.Documents)
                {
                    var csDoc = doc as CSDocument;
                    if (csDoc != null)
                    {
                        // Rename the CSDocument
                        if (ContainsNodeInfo(csDoc, nodeInfo))
                        {
                            var newDocName = string.Format("{0}.cs", Path.GetFileNameWithoutExtension(className));
                            csDoc.Name = newDocName;
                            csDoc.Rename(newDocName);
                            break;
                        }
                    }
                }

                foreach (var doc in newProject.Documents)
                {
                    // Now scan the document for possible plugin infos.
                    // If we find one, update its properties and rename the class.
                    var csDoc = doc as CSDocument;
                    if (csDoc != null)
                    {
                        var parserResults = csDoc.Parse(true);
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

                // Save the project.
                newProject.Save();

                newFilename = newProject.LocalPath;
                return true;
            }
        }
        
        private static bool ContainsNodeInfo(CSDocument document, INodeInfo nodeInfo)
        {
            var parseInfo = document.ParseInfo;
            var compilationUnit = parseInfo.MostRecentCompilationUnit;
            if (compilationUnit == null) return false;
            
            foreach (var clss in compilationUnit.Classes)
            {
                foreach (var attribute in clss.Attributes)
                {
                    var attributeType = attribute.AttributeType;
                    var pluginInfoName = typeof(PluginInfoAttribute).Name;
                    var pluginInfoShortName = pluginInfoName.Replace("Attribute", "");
                    if (attributeType.Name == pluginInfoName || attributeType.Name == pluginInfoShortName)
                    {
                        // Check name
                        string name = null;
                        if (attribute.NamedArguments.ContainsKey("Name"))
                            name = (string) attribute.NamedArguments["Name"];
                        else if (attribute.PositionalArguments.Count >= 0)
                            name = (string) attribute.PositionalArguments[0];
                        
                        if (name != nodeInfo.Name)
                            continue;
                        
                        // Check category
                        string category = null;
                        if (attribute.NamedArguments.ContainsKey("Category"))
                            category = (string) attribute.NamedArguments["Category"];
                        else if (attribute.PositionalArguments.Count >= 1)
                            category = (string) attribute.PositionalArguments[1];
                        
                        if (category != nodeInfo.Category)
                            continue;

                        // Possible match
                        bool match = true;
                        
                        // Check version
                        if (!string.IsNullOrEmpty(nodeInfo.Version))
                        {
                            string version = null;
                            if (attribute.NamedArguments.ContainsKey("Version"))
                                version = (string) attribute.NamedArguments["Version"];
                            else if (attribute.PositionalArguments.Count >= 2)
                                version = (string) attribute.PositionalArguments[2];
                            
                            match = version == nodeInfo.Version;
                        }
                        
                        if (match)
                            return true;
                    }
                }
            }
            
            return false;
        }
    }

    internal class PluginClassTransformer : AbstractAstTransformer
    {
        static string PLUGIN_INFO = typeof(PluginInfoAttribute).Name.Replace("Attribute", "");
        static string NAME = "Name";
        static string CATEGORY = "Category";
        static string VERSION = "Version";
        static string HELP = "Help";
        static string TAGS = "Tags";

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
        
        private static PluginInfoAttribute ConvertAttributeToPluginInfo(ICSharpCode.NRefactory.Ast.Attribute attribute)
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

            //when cloning from a template
            //remove the dummy helps and tags
            var removeHelpAndTags = false;
            var oldArguments = new List<NamedArgumentExpression>();
            foreach (var argument in namedArguments)
                if (argument.Name == NAME)
                {
                    oldArguments.Add(argument);
                    var expr = (PrimitiveExpression)argument.Expression;
                    if (expr.StringValue.StartsWith("\"Template"))
                        removeHelpAndTags = true;
                }
                else if (argument.Name == CATEGORY || argument.Name == VERSION || argument.Name == HELP || argument.Name == TAGS)
                    oldArguments.Add(argument);

            foreach (var argument in oldArguments)
                if (argument.Name == HELP || argument.Name == TAGS)
                {
                    if (removeHelpAndTags)
                        namedArguments.Remove(argument);
                }
                else
                    namedArguments.Remove(argument);

            namedArguments.Insert(0, new NamedArgumentExpression(NAME, new PrimitiveExpression(FName, FName)));
            namedArguments.Insert(1, new NamedArgumentExpression(CATEGORY, new PrimitiveExpression(FCategory, FCategory)));
            if (!string.IsNullOrEmpty(FVersion))
                namedArguments.Insert(2, new NamedArgumentExpression(VERSION, new PrimitiveExpression(FVersion, FVersion)));
        }
    }
}
