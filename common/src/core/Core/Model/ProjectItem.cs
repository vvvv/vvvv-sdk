
using System;

namespace VVVV.Core.Model
{
	public abstract class ProjectItem : IDItem, IProjectItem
	{
		public ProjectItem(string name)
			: base(name)
		{
		}
		
		public virtual IProject Project 
		{
			get;
			set;
		}
		
		public virtual bool CanBeCompiled 
		{
			get 
			{
				return false;
			}
		}
	}
}
