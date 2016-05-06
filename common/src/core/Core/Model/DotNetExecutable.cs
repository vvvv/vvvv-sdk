using System;
using System.Reflection;
using VVVV.Core.Model;

namespace VVVV.Core.Model
{
	public class DotNetExecutable : IExecutable
	{
		public IProject Project 
		{
			get;
			set;
		}
		
		public Assembly Assembly 
		{
			get;
			set;
		}
		
		public DotNetExecutable(IProject project)
		{
			Project = project;
			Assembly = null;
		}
		
		public DotNetExecutable(IProject project, Assembly assembly)
		{
			Project = project;
			Assembly = assembly;
		}
	}
}
