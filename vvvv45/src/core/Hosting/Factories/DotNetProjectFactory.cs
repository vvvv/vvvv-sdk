using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using VVVV.Core.Logging;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
	[Export(typeof(IAddonFactory))]
	public class DotNetProjectFactory : AbstractFileFactory
	{
		[Import]
		protected DotNetPluginFactory FPluginFactory;
		
		[Import]
		protected ISolution FSolution;
		
		[Import]
		protected ILogger FLogger;
		
		private Dictionary<string, IProject> FProjects;
		
		public DotNetProjectFactory()
		{
			FFileExtension = ".csproj";
			FDirectory = Path.Combine(FDirectory, @"..\..\dynamic");
			
			FProjects = new Dictionary<string, IProject>();
		}
		
		public override IEnumerable<INodeInfo> ExtractNodeInfos(string filename)
		{
		    if (Path.GetExtension(filename) != FFileExtension) yield break;
		    
			IProject project;
			
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
	}
}
