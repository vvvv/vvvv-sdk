using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public abstract class Argument : IScriptGenerator
	{

		public abstract string PScript
		{
			get;
		}
	}
}
