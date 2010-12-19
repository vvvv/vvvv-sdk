using System;
using System.Collections.Generic;
using System.Linq;

using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.View;
using VVVV.HDE.ProjectExplorer.NodeModel;
using VVVV.Utils.Linq;

namespace VVVV.HDE.ProjectExplorer
{
	public class LoadedProjectsSolutionViewProvider : SolutionViewProvider
	{
		private Node FRootNode;
		
		public LoadedProjectsSolutionViewProvider(ISolution solution, Node rootNode, ModelMapper mapper)
			: base(solution, mapper)
		{
			FRootNode = rootNode;
			
			node_Added(null, FRootNode);
		}
		
		void node_Added(IViewableCollection<Node> collection, Node item)
		{
			item.Added += node_Added;
			item.Removed += node_Removed;
			
			foreach (var node in item)
				node_Added(item, node);
			
			var project = item.NodeInfo.UserData as IProject;
			if (project != null)
				OnOrderChanged();
		}

		void node_Removed(IViewableCollection<Node> collection, Node item)
		{
			item.Added -= node_Added;
			item.Removed -= node_Removed;
			
			foreach (var node in item)
				node_Removed(item, node);
			
			var project = item.NodeInfo.UserData as IProject;
			if (project != null)
				OnOrderChanged();
		}

		public override void Dispose()
		{
			foreach (var node in FRootNode.AsDepthFirstEnumerable())
			{
				node.Added -= node_Added;
				node.Removed -= node_Removed;
			}
			base.Dispose();
		}
		
		public override void Add(IProject item)
		{
			if (IsProjectInUse(item))
				base.Add(item);
		}
		
		protected bool IsProjectInUse(IProject project)
		{
			var query =
				from node in FRootNode.AsDepthFirstEnumerable()
				where node.NodeInfo.UserData == project
				select node;
			
			return query.Any();
		}

		public override IEnumerator<IProject> GetEnumerator()
		{
			foreach (var project in FSolution.Projects)
			{
				if (IsProjectInUse(project))
					yield return project;
			}
		}
		
		public override IProject this[int index] 
		{
			get 
			{
				return FSolution.Projects.Where(project => IsProjectInUse(project)).ToList()[index];
			}
		}
		
		public override int Count 
		{
			get 
			{ 
				return FSolution.Projects.Count(project => IsProjectInUse(project));
			}
		}
	}
}
