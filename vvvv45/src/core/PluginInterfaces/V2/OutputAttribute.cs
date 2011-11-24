using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
	public sealed class OutputAttribute : IOAttribute
	{
		public OutputAttribute(string name)
			:base(name)
		{
		}
		
		public override object Clone()
		{
			return base.Clone(new OutputAttribute(Name));
		}
	}
}
