using System;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
	public class InputAttribute : PinAttribute
	{
		public InputAttribute(string name)
			:base(name)
		{
			IsFast = false;
		}
		
		public bool IsFast
		{
			get;
			set;
		}
	}
}
