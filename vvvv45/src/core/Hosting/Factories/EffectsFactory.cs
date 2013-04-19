using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.CodeDom.Compiler;
using System.IO;

using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
    /// <summary>
    /// Effects factory, parses and watches the effect directory
    /// </summary>
    [Export(	typeof(IAddonFactory))]
    [Export(typeof(EffectsFactory))]
    [ComVisible(false)]
    public class EffectsFactory : AbstractFileFactory<IEffectHost>
    {

        [Import]
        protected ISolution FSolution;
        
        [Import]
        protected ILogger Logger { get; set; }
        
        private readonly Dictionary<string, FXProject> FProjects;
        
        [ImportingConstructor]
        public EffectsFactory(INodeInfoFactory nodeInfoFactory)
            : base(".fx;.xx")
        {
            FProjects = new Dictionary<string, FXProject>();
            // Listen to stuff added by nodelist.xml
            nodeInfoFactory.NodeInfoAdded += HandleNodeInfoAdded;
        }

        void HandleNodeInfoAdded(object sender, INodeInfo nodeInfo)
        {
            if (nodeInfo.Type == NodeType.Effect && nodeInfo.Factory == this)
            {
                nodeInfo.UserData = CreateProject(nodeInfo.Filename);
            }
        }
        
        public override string JobStdSubPath {
            get {
                return "effects";
            }
        }
        
        //create a node info from a filename
        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            var project = CreateProject(filename);
            if (project != null)
            	yield return LoadNodeInfoFromEffect(filename, project);
        }
        
        protected override void DoAddFile(string filename)
        {
        	if (CreateProject(filename) != null)
            	base.DoAddFile(filename);
        }
        
        protected override void DoRemoveFile(string filename)
        {
            FXProject project;
            if (FProjects.TryGetValue(filename, out project))
            {
                if (FSolution.Projects.CanRemove(project))
                {
                    FSolution.Projects.Remove(project);
                    project.DoCompileEvent -= project_DoCompileEvent;
                }
                FProjects.Remove(filename);
            }
            
            base.DoRemoveFile(filename);
        }
        
        private FXProject CreateProject(string filename)
        {
            FXProject project;
            if (!FProjects.TryGetValue(filename, out project))
            {
            	var isDX9 = true;
            	//check if this is a dx9 effect in that it does not contain "technique10" or "technique11"
            	using (StreamReader sr = new StreamReader(filename))
                {
            		string line;
            		var t10 = "technique10";
            		var t11 = "technique11";
                    
                    // Parse lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                    	if (line.Contains(t10) || line.Contains(t11))
                    	{
                    		isDX9 = false;
                    		break;
                    	}
                    }
            	}
            	
            	if (isDX9)
	            {
	                project = new FXProject(filename, FHDEHost.ExePath);
	                if (FSolution.Projects.CanAdd(project))
	                {
	                    FSolution.Projects.Add(project);
	                    //effects are actually being compiled by vvvv when nodeinfo is update
	                    //so we need to intervere with the doCompile
	                    project.DoCompileEvent += project_DoCompileEvent;
	                    //in turn not longer needs the following:
	                    //project.ProjectCompiledSuccessfully += project_ProjectCompiledSuccessfully;
	                }
	                else
	                {
	                    // Project was renamed
	                    project = FSolution.Projects[project.Name] as FXProject;
	                }
	                
                	FProjects[filename] = project;
            	}
            }
            
            return project;
        }

        void project_DoCompileEvent(object sender, EventArgs e)
        {
            var project = sender as FXProject;
            var filename = project.LocalPath;
            
            LoadNodeInfoFromEffect(filename, project);
        }

//		void project_ProjectCompiledSuccessfully(object sender, CompilerEventArgs args)
//		{
//			var project = sender as FXProject;
//			var filename = project.Location.LocalPath;
//
//			LoadNodeInfoFromEffect(filename);
//		}
        
        private INodeInfo LoadNodeInfoFromEffect(string filename, FXProject project)
        {
            var nodeInfo = FNodeInfoFactory.CreateNodeInfo(
                Path.GetFileNameWithoutExtension(filename),
                "EX9.Effect",
                string.Empty,
                filename,
                true);
            
            nodeInfo.Type = NodeType.Effect;
            nodeInfo.Factory = this;
            nodeInfo.UserData = project;
            
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    string author = @"//@author:";
                    string desc = @"//@help:";
                    string tags = @"//@tags:";
                    string credits = @"//@credits:";
                    
                    // Parse lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith(author))
                            nodeInfo.Author = line.Replace(author, "").Trim();
                        
                        else if (line.StartsWith(desc))
                            nodeInfo.Help = line.Replace(desc, "").Trim();
                        
                        else if (line.StartsWith(tags))
                            nodeInfo.Tags = line.Replace(tags, "").Trim();
                        
                        else if (line.StartsWith(credits))
                            nodeInfo.Credits = line.Replace(credits, "").Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, "Effect does not contain detailed info");
                Logger.Log(ex);
            }

            nodeInfo.CommitUpdate();
            
            return nodeInfo;
        }
        
        protected override bool CreateNode(INodeInfo nodeInfo, IEffectHost effectHost)
        {
            if (nodeInfo.Type != NodeType.Effect)
                return false;
            
            var project = nodeInfo.UserData as FXProject;
            
            //get the code of the FXProject associated with the nodeinfos filename
            effectHost.SetEffect(nodeInfo.Filename, project.Code);

            //now the effect is compiled in vvvv and we can access the errors
            string e = effectHost.GetErrors();
            if (string.IsNullOrEmpty(e))
                e = "";
            
            var compilerResults = new CompilerResults(null);
            //now parse errors to CompilerResults
            //split errorstring linewise
            var errorlines = e.Split(new char[1]{'\n'});
            foreach (var line in errorlines)
            {
                string filePath = project.LocalPath;
                string eCoords = string.Empty;
                int eLine = 0;
                int eChar = 0;
                string eNumber = string.Empty;
                string eText = string.Empty;
                bool isWarning = false;
                
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                
                //split the line at ": "
                //which results in 3 or 4 lines:
                //[0] filename (line, character)
                //[1] error/warning code
                //[2] (optional) erroneous character
                //[3] error description
                var eItems = line.Split(new string[1]{": "}, StringSplitOptions.None);
                
                //extract line/char substring
                int start = eItems[0].LastIndexOf('(');
                int end = eItems[0].LastIndexOf(')');
                
                if (start > 0)
                {
                    string relativePath = eItems[0].Substring(0, start);
                    
                    //if this is a path to an include..
                    if (relativePath != Path.Combine(FHDEHost.ExePath, "memory"))
                    {
                        // we need to guess here. shader compiler outputs relative paths.
                        // we don't know if the include was "local" or <global>
                        
                        filePath = Path.Combine(Path.GetDirectoryName(project.LocalPath), relativePath);
                        if (!File.Exists(filePath))
                        {
                            string fileName = Path.GetFileName(relativePath);
                            
                            foreach (var reference in project.References)
                            {
                                var referenceFileName = Path.GetFileName((reference as FXReference).ReferencedDocument.LocalPath);
                                if (referenceFileName.ToLower() == fileName.ToLower())
                                {
                                    filePath = reference.AssemblyLocation;
                                }
                            }
                        }
                    }
                }
                
                if (start > 0 && end > 0)
                {
                    eCoords = eItems[0].Substring(start+1, end-start-1);
                    var eLineChar = eCoords.Split(new char[1]{','});
                    eLine = Convert.ToInt32(eLineChar[0]);
                    eChar = Convert.ToInt32(eLineChar[1]);
                    
                    if (eItems[1].StartsWith("warning"))
                    {
                        isWarning = true;
                        eNumber = eItems[1].Substring(8, 5);
                    }
                    else
                        eNumber = eItems[1].Substring(6, 5);
                    
                    eText = eItems[2];
                    if (eItems.Length > 3)
                        eText += ": " + eItems[3];
                }
                else
                {
                    eText = line;
                }
                
                var error = new CompilerError(filePath, eLine, eChar, eNumber, eText);
                error.IsWarning = isWarning;
                compilerResults.Errors.Add(error);
            }
            
            project.CompilerResults = compilerResults;
            
            //and the input pins
            string f = effectHost.GetParameterDescription();
            if (string.IsNullOrEmpty(f))
                f = "";
            project.ParameterDescription = f;
            
            return true;
        }
        
        protected override bool DeleteNode(INodeInfo nodeInfo, IEffectHost host)
        {
            return true;
        }
        
        protected override bool CloneNode(INodeInfo nodeInfo, string path, string name, string category, string version, out string filename)
        {
            if (nodeInfo.Type == NodeType.Effect)
            {
                var project = nodeInfo.UserData as FXProject;
                var projectDir = path;
                var newProjectName = name + ".fx";
                filename = projectDir.ConcatPath(newProjectName);

                project.SaveTo(filename);
                
                return true;
            }
            
            return base.CloneNode(nodeInfo, path, name, category, version, out filename);
        }
    }
}
