using System;
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
	[Export(typeof(IAddonFactory))]
	public class EffectsFactory : AbstractFileFactory<IEffectHost>
	{
		[Import]
		protected INodeInfoFactory FNodeInfoFactory;
		
		[Import]
		protected ISolution FSolution;
		
		[Import]
		protected ILogger Logger { get; set; }
		
		private Dictionary<string, FXProject> FProjects;
		private Dictionary<FXProject, INodeInfo> FProjectNodeInfo;
		
		public EffectsFactory()
			: base(".fx")
		{
			FProjects = new Dictionary<string, FXProject>();
			FProjectNodeInfo = new Dictionary<FXProject, INodeInfo>();
		}
		
		public override string JobStdSubPath {
			get {
				return "effects";
			}
		}
		
		//create a node info from a filename
		protected override IEnumerable<INodeInfo> GetNodeInfos(string filename)
		{
			FXProject project;
			if (!FProjects.TryGetValue(filename, out project))
			{
				project = new FXProject(new Uri(filename));
				if (FSolution.Projects.CanAdd(project))
				{
					FSolution.Projects.Add(project);
					project.DoCompileEvent += new EventHandler(project_DoCompile);
					FProjects[filename] = project;
				}
			}
			
			var isLoaded = project.IsLoaded;
			if (!isLoaded)
				project.Load();
			
			project.Compile();
			
			if (!isLoaded)
				project.Unload();

			yield return FProjectNodeInfo[project];
		}
		
		void project_DoCompile(object sender, EventArgs e)
		{
			//parse nodeinfo of project
			var project = (sender as FXProject);
			string filename = project.Location.LocalPath;
			
			var nodeInfo = FNodeInfoFactory.CreateNodeInfo(
				Path.GetFileNameWithoutExtension(filename),
				"EX9.Effect",
				string.Empty,
				filename);
			
			nodeInfo.BeginUpdate();
			nodeInfo.Type = NodeType.Effect;
//			nodeInfo.Executable = project.Exectuable;
			
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
				Logger.Log(LogType.Error, "Could not extract effect info");
				Logger.Log(ex);
			}

			//for being available in the ExtractNodeInfo call
			if (!FProjectNodeInfo.ContainsKey(project))
				FProjectNodeInfo.Add(project, nodeInfo);
			
			nodeInfo.CommitUpdate();
			
			//re-register nodeinfo with vvvv
			OnNodeInfoUpdated(nodeInfo);
		}
		
		protected override bool CreateNode(INodeInfo nodeInfo, IEffectHost effectHost)
		{
			if (nodeInfo.Type != NodeType.Effect)
				return false;
			
			var project = FProjects[nodeInfo.Filename];
			if (!project.IsLoaded)
				project.Load();
			
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
				string filename = project.Location.LocalPath;
				int eLine;
				string eNumber;
				string eText = "";
				
				//split the line at :
				var eItems = line.Split(new char[1]{':'});
				int start = eItems[0].IndexOf('(');
				int end = eItems[0].IndexOf(')');
				
				//if there is no linenumber in braces continue with the next error
				//this may be a warning
				if (start == -1)
					continue;
				
				if (start > 0)
					filename = Path.Combine(Path.GetDirectoryName(project.Location.LocalPath), eItems[0].Substring(0, start));
				
				eLine = Convert.ToInt32(eItems[0].Substring(start+1, end-start-1));
				
				eNumber = eItems[1].Substring(7, 5);
				
				for (int i = 2; i < eItems.Length; i++)
					eText += eItems[i];
				
				compilerResults.Errors.Add(new CompilerError(filename, eLine, 0, eNumber, eText));
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
				var project = FProjects[nodeInfo.Filename];
				if (!project.IsLoaded)
					project.Load();
				
				var newProject = project.Clone() as FXProject;
				
				var projectDir = path; //project.Location.GetLocalDir();
				var newProjectName = name + ".fx";
				var newLocation = new Uri(projectDir.ConcatPath(newProjectName));
				newProject.Location = newLocation;
				
				var newLocationDir = newLocation.GetLocalDir();
				foreach (var doc in newProject.Documents)
				{
					// The documents are cloned but their location still refers to the old one.
					// Only clone FX files, not FXH.
					// FXProjects are saved as one FXDocument.
					if (doc is FXDocument)
					{
						doc.Location = newLocation;
						doc.Save();
						break;
					}
				}
				
				// Save the project.
				newProject.Save();
				
				filename = newProject.Location.LocalPath;
				return true;
			}
			
			return base.CloneNode(nodeInfo, path, name, category, version, out filename);
		}
	}
}
