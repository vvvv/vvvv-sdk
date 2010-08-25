
using System;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
	public class OutputAttribute : PinAttribute
	{
		public OutputAttribute(string name)
			:base(name)
		{
		}
	}
}
