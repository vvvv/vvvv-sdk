using System;
using System.ComponentModel.Composition;

namespace VVVV.PluginInterfaces.V2
{
	public abstract class PinAttribute : ImportAttribute
	{
		public PinAttribute(PinDirection direction)
		{
			Direction = direction;
		}
		
		public PinDirection Direction
		{
			get;
			private set;
		}
	}
}
