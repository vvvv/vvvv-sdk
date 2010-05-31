using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

using VVVV.HDE.Model;

using Dom = ICSharpCode.SharpDevelop.Dom;

namespace VVVV.Nodes
{
	public class BackgroundParser
	{
		private BackgroundWorker FBackgroundWorker;
		private Dom.ProjectContentRegistry FPCRegistry;
		private Dictionary<IProject, Dom.DefaultProjectContent> FProjects;
		private ToolStripStatusLabel FParserLabel;
		
		public BackgroundParser(Dom.ProjectContentRegistry pcRegistry, Dictionary<IProject, Dom.DefaultProjectContent> projects, ToolStripStatusLabel parserLabel)
		{
			FBackgroundWorker = new BackgroundWorker();
			FBackgroundWorker.WorkerReportsProgress = true;
			FBackgroundWorker.WorkerSupportsCancellation = true;
			
			FBackgroundWorker.DoWork += new DoWorkEventHandler(DoWorkCB);
			FBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompletedCB);
			FBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChangedCB);
		
			FPCRegistry = pcRegistry;
			FProjects = projects;
			FParserLabel = parserLabel;
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
			int i = 0;
			int percentProgress = 0;
			
			foreach (var entry in FProjects)
			{
				var project = entry.Key;
				var projectContent = entry.Value;
				projectContent.AddReferencedContent(FPCRegistry.Mscorlib);
				
				percentProgress = (i++) * 100;
			
				int j = 0;
				int percentInnerProgress = 0;
				foreach (var reference in project.References)
				{
					if (FBackgroundWorker.CancellationPending)
					{
						args.Cancel = true;
						return;
					}
					
					if (reference is AssemblyReference)
					{
						var assemblyName = reference.Name;
						var assemblyFilename = reference.Location.AbsolutePath;
						var referencePC = FPCRegistry.GetProjectContentForReference(assemblyName, assemblyFilename);
						projectContent.AddReferencedContent(referencePC);
					}
					
					percentInnerProgress = percentProgress + ((j++) * 100) / project.References.Count;
					FBackgroundWorker.ReportProgress(percentInnerProgress / FProjects.Count, reference.Name);
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
	}
}
