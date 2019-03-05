using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;
using VVVV.Core.Model;
using VVVV.Core.View;
using VVVV.Hosting.Graph;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.Linq;

namespace VVVV.HDE.ProjectExplorer
{
	public class LoadedProjectsSolutionViewProvider : IParent, IDisposable
	{
		private readonly INode2 FRootNode;
		private readonly EditableList<IProject> FLoadedProjects;
//		private readonly SortedViewableList<IProject, string> FSortedLoadedProjects;
		private readonly Synchronizer<IProject, IProject> FSynchronizer;
		
		public LoadedProjectsSolutionViewProvider(ISolution solution, INode2 rootNode, ModelMapper mapper)
//			: base(solution, mapper)
		{
			FRootNode = rootNode;
			FLoadedProjects = new EditableList<IProject>();
			FSynchronizer = FLoadedProjects.SyncWith(solution.Projects, p => p, p => {}, IsProjectInUse);
			Childs = new SortedViewableList<IProject, string>(FLoadedProjects, project => project.Name);
			
			node_Added(null, FRootNode);
		}
		
		public void Dispose()
		{
			foreach (var node in FRootNode.AsDepthFirstEnumerable())
			{
				node.Added -= node_Added;
				node.Removed -= node_Removed;
			}
			
			((SortedViewableList<IProject, string>) Childs).Dispose();
			FSynchronizer.Dispose();
			FLoadedProjects.Dispose();
		}
		
		void node_Added(IViewableCollection<INode2> collection, INode2 item)
		{
			item.Added += node_Added;
			item.Removed += node_Removed;
			
			foreach (var node in item)
				node_Added(item, node);
			
			var project = item.NodeInfo?.UserData as IProject;
			if (project != null && !FLoadedProjects.Contains(project))
			{
			    FLoadedProjects.Add(project);
			}
		}

		void node_Removed(IViewableCollection<INode2> collection, INode2 item)
		{
			item.Added -= node_Added;
			item.Removed -= node_Removed;
			
			foreach (var node in item)
				node_Removed(item, node);
			
			var project = item.NodeInfo?.UserData as IProject;
			if (project != null && !IsProjectInUse(project))
			{
				FLoadedProjects.Remove(project);
			}
		}

		protected bool IsProjectInUse(IProject project)
		{
			var query =
				from node in FRootNode.AsDepthFirstEnumerable()
				where node.NodeInfo?.UserData == project
				select node;
			
			return query.Any();
		}
	    
        public System.Collections.IEnumerable Childs
        {
            get;
            private set;
        }
	}
}
