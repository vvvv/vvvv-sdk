using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

using VVVV.Core.Model;

using Dom = ICSharpCode.SharpDevelop.Dom;

namespace VVVV.HDE.CodeEditor
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
		private IParseInfoProvider FParseInfoProvider;
		
		public BackgroundParser(Dom.ProjectContentRegistry pcRegistry, IParseInfoProvider parseInfoProvider)
		{
			FWorkerQueue = new Queue<Tuple<BackgroundWorker, ICollection<IProject>>>();
		
			FPCRegistry = pcRegistry;
			FParseInfoProvider = parseInfoProvider;
		}
		
		public void Parse(IProject project)
		{
			var projects = new List<IProject>();
			projects.Add(project);
			Parse(projects);
		}
		
		public void Parse(IEnumerable<IProject> projects)
		{
			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = false;
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += DoWorkCB;
			worker.RunWorkerCompleted += RunWorkerCompletedCB;
			
			FWorkerQueue.Enqueue(new Tuple<BackgroundWorker, ICollection<IProject>>(worker, projects.ToList()));
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
			
			foreach (var project in projects)
			{
				// Get the IProjectContent for this project
				var projectContent = FParseInfoProvider.GetProjectContent(project);
				
				// Clear all referenced content
				lock(projectContent.ReferencedContents)
				{
					projectContent.ReferencedContents.Clear();
				}
				
				// Add mscorlib
				projectContent.AddReferencedContent(FPCRegistry.Mscorlib);
				
				// Parse all references
				foreach (var reference in project.References)
				{
					if (worker.CancellationPending)
					{
						args.Cancel = true;
						return;
					}
					
					if (reference is ProjectReference)
					{
						var projectReference = reference as ProjectReference;
						var referencePC = FParseInfoProvider.GetProjectContent(projectReference.Project);
						projectContent.AddReferencedContent(referencePC);
					}
					else if (reference is IReference)
					{
						var assemblyName = reference.Name;
						var assemblyFilename = reference.AssemblyLocation;
						var referencePC = FPCRegistry.GetProjectContentForReference(assemblyName, assemblyFilename);
						projectContent.AddReferencedContent(referencePC);
					}
				}
				
				// Parse the document itself (all documents)
				foreach (var doc in project.Documents)
				{
					var parser = new CodeParser(FParseInfoProvider);
					if (doc is ITextDocument)
						parser.Parse(doc as ITextDocument);
				}
			}
		}
		
		void RunWorkerCompletedCB(object sender, RunWorkerCompletedEventArgs args)
		{
			if (FWorkerQueue.Count > 0)
			{
				var tuple = FWorkerQueue.Dequeue();
				tuple.Fst.RunWorkerAsync(tuple.Snd);
			}
		}
	}
}
