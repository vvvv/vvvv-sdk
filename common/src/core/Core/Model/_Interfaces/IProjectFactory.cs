using System;
using System.Collections.Generic;

namespace VVVV.Core.Model
{
	public interface IProjectFactory 
	{
	    bool CreateProjectFrom(string filename, out IProject project);
	}
}
