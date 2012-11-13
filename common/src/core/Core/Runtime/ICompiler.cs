using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;

using VVVV.Core.Model;

namespace VVVV.Core.Runtime
{
	public interface ICompiler<T> where T: IProject
	{
	    CompilerResults Compile(T project);
	}
}
