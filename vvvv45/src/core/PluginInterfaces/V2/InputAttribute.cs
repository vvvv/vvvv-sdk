using System;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
	public class InputAttribute : PinAttribute
	{
		public InputAttribute(string name)
			:base(PinDirection.Input)
		{
			Name = name;
		}
		
		public string Name 
		{ 
			get; 
			set; 
		}
	}
}
