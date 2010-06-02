using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

using VVVV.HDE.Model;
using VVVV.Utils.Concurrent;

using Dom = ICSharpCode.SharpDevelop.Dom;

namespace VVVV.Nodes
{
	public class BackgroundParser
	{
		class Tuple<T1, T2>
		{
			public T1 Fst { get; set; }
			public T2 Snd { get; set; }
			
			public Tuple(T1 fst, T2 snd)
			{
				Fst = fst;
				Snd = snd;
			}
		}
		
		private Queue<Tuple<BackgroundWorker, ICollection<IProject>>> FWorkerQueue;
		private Dom.ProjectContentRegistry FPCRegistry;
		private Dictionary<IProject, Dom.DefaultProjectContent> FProjects;
		private ToolStripStatusLabel FParserLabel;
		
		public BackgroundParser(Dom.ProjectContentRegistry pcRegistry, Dictionary<IProject, Dom.DefaultProjectContent> projects, ToolStripStatusLabel parserLabel)
		{
			FWorkerQueue = new Queue<Tuple<BackgroundWorker, ICollection<IProject>>>();
		
			FPCRegistry = pcRegistry;
			FProjects = projects;
			FParserLabel = parserLabel;
		}
		
		public void Parse(IProject project)
		{
			var projects = new List<IProject>();
			projects.Add(project);
			Parse(projects);
		}
		
		public void Parse(ICollection<IProject> projects)
		{
			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += DoWorkCB;
			worker.ProgressChanged += ProgressChangedCB;
			worker.RunWorkerCompleted += RunWorkerCompletedCB;
			
			FWorkerQueue.Enqueue(new Tuple<BackgroundWorker, ICollection<IProject>>(worker, projects));
			if (FWorkerQueue.Count == 1)
				worker.RunWorkerAsync(projects);
		}
		
		public void CancelAsync()
		{
			foreach (var tuple in FWorkerQueue)
			{
				tuple.Fst.CancelAsync();
			}
		}
		
		void DoWorkCB(object sender, DoWorkEventArgs args)
		{
			var worker = sender as BackgroundWorker;
			var projects = args.Argument as ICollection<IProject>;
			
			int i = 0;
			int percentProgress = 0;
			
			foreach (var project in projects)
			{
				// Get the IProjectContent for this project
				var projectContent = FProjects[project];
				
				// Clear all referenced content
				lock(projectContent.ReferencedContents)
				{
					projectContent.ReferencedContents.Clear();
				}
				
				// Add mscorlib
				projectContent.AddReferencedContent(FPCRegistry.Mscorlib);
				
				percentProgress = (i++) * 100;
			
				int j = 0;
				int percentInnerProgress = 0;
				foreach (var reference in project.References)
				{
					if (worker.CancellationPending)
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
					else if (reference is ProjectReference)
					{
						var projectReference = reference as ProjectReference;
						var referencePC = FProjects[projectReference.Project];
						projectContent.AddReferencedContent(referencePC);
					}
					
					percentInnerProgress = percentProgress + ((j++) * 100) / project.References.Count;
					worker.ReportProgress(percentInnerProgress / projects.Count, reference.Name);
				}
			}
		}
		
		void RunWorkerCompletedCB(object sender, RunWorkerCompletedEventArgs args)
		{
			if (!FParserLabel.IsDisposed)
				FParserLabel.Text = "Ready";
			
			if (FWorkerQueue.Count > 0)
			{
				var tuple = FWorkerQueue.Dequeue();
				tuple.Fst.RunWorkerAsync(tuple.Snd);
			}
		}
		
		void ProgressChangedCB(object sender, ProgressChangedEventArgs args)
		{
			string assemblyName = args.UserState as string;
			if (!FParserLabel.IsDisposed)
				FParserLabel.Text = "Loading " + assemblyName + " ...";
		}
	}
}
