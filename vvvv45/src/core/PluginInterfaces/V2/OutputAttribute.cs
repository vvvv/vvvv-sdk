using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public class OutputAttribute : PinAttribute
	{
		public OutputAttribute(string name)
			:base(name)
		{
		}
	}
}
