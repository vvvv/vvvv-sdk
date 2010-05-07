using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

using Dom = ICSharpCode.SharpDevelop.Dom;

namespace VVVV.Nodes
{
	public class BackgroundParser
	{
		private BackgroundWorker FBackgroundWorker;
		private Dom.ProjectContentRegistry FPCRegistry;
		private Dom.DefaultProjectContent FProjectContent;
		private ToolStripStatusLabel FParserLabel;
		private Dictionary<string, string> FAssemblyFileMap;
		
		public BackgroundParser(Dom.ProjectContentRegistry pcRegistry, Dom.DefaultProjectContent projectContent, ToolStripStatusLabel parserLabel, Dictionary<string, string> assemblyTable)
		{
			FBackgroundWorker = new BackgroundWorker();
			FBackgroundWorker.WorkerReportsProgress = true;
			FBackgroundWorker.WorkerSupportsCancellation = true;
			
			FBackgroundWorker.DoWork += new DoWorkEventHandler(DoWorkCB);
			FBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompletedCB);
			FBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChangedCB);
		
			FPCRegistry = pcRegistry;
			FProjectContent = projectContent;
			FParserLabel = parserLabel;
			
			FAssemblyFileMap = assemblyTable;
		}
		
		public void RunParserAsync()
		{
			FBackgroundWorker.RunWorkerAsync();
		}
		
		public void CancelAsync()
		{
			FBackgroundWorker.CancelAsync();
		}
		
		private void DoWorkCB(object sender, DoWorkEventArgs args)
		{
			string[] referencedAssemblies = {
				"mscorlib", "System", "System.Data", "System.Drawing", "System.Xml", "System.Windows.Forms", "PluginInterfaces", "_Utils"
			};
			
			int i = 0;
			int percentProgress = 0;
			
			foreach (string assemblyName in referencedAssemblies) {
				if (FBackgroundWorker.CancellationPending)
				{
					args.Cancel = true;
					break;
				}
				
				percentProgress = ((i++) * 100) / referencedAssemblies.Length;
				FBackgroundWorker.ReportProgress(percentProgress, assemblyName);
				
				string assemblyFilename = ResolveAssemblyFilename(assemblyName);
				Dom.IProjectContent referencePC = FPCRegistry.GetProjectContentForReference(assemblyName, assemblyFilename);
				FProjectContent.AddReferencedContent(referencePC);
				if (referencePC is Dom.ReflectionProjectContent) {
					(referencePC as Dom.ReflectionProjectContent).InitializeReferences();
				}
			}
		}
		
		private void RunWorkerCompletedCB(object sender, RunWorkerCompletedEventArgs args)
		{
			FParserLabel.Text = "Ready";
		}
		
		private void ProgressChangedCB(object sender, ProgressChangedEventArgs args)
		{
			string assemblyName = args.UserState as string;
			FParserLabel.Text = "Loading " + assemblyName + " ...";
		}
		
		private string ResolveAssemblyFilename(string assemblyName)
		{
			if (FAssemblyFileMap.ContainsKey(assemblyName))
			{
				return FAssemblyFileMap[assemblyName];
			}
			return assemblyName;
		}
	}
}
