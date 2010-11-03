using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.View;

namespace VVVV.HDE.ProjectExplorer
{
	public class LoadedProjectsSolutionViewProvider : SolutionViewProvider
	{
		public LoadedProjectsSolutionViewProvider(ISolution solution, ModelMapper mapper)
			: base(solution, mapper)
		{
			foreach (var project in solution.Projects)
			{
				project.Loaded += project_Loaded;
				project.Unloaded += project_Unloaded;
			}
		}

		void project_Unloaded(object sender, EventArgs e)
		{
			OnOrderChanged();
		}

		void project_Loaded(object sender, EventArgs e)
		{
			OnOrderChanged();
		}
		
		protected override void OnAdded(IProject item)
		{
			item.Loaded += project_Loaded;
			item.Unloaded += project_Unloaded;
			base.OnAdded(item);
		}
		
		protected override void OnRemoved(IProject item)
		{
			item.Loaded -= project_Loaded;
			item.Unloaded -= project_Unloaded;
			base.OnRemoved(item);
		}
		
		public override IEnumerator<IProject> GetEnumerator()
		{
			foreach (var project in FSolution.Projects)
			{
				if (project.IsLoaded)
					yield return project;
			}
		}
		
		public override IProject this[int index] 
		{
			get 
			{
				return FSolution.Projects.Where(project => project.IsLoaded).ToList()[index];
			}
		}
		
		public override int Count 
		{
			get 
			{ 
				return FSolution.Projects.Count(project => project.IsLoaded);
			}
		}
	}
}
