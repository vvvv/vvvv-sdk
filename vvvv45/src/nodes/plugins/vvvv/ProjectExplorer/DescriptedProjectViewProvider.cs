
using System;
using VVVV.Core.Model;
using VVVV.Core.View;

namespace VVVV.HDE.ProjectExplorer
{
	public class DescriptedProjectViewProvider : IDescripted
	{
		protected IProject FProject;
		
		public DescriptedProjectViewProvider(IProject project)
		{
			FProject = project;
		}
		
		public string Description {
			get {
				return FProject.LocalPath;
			}
		}
	}
}
