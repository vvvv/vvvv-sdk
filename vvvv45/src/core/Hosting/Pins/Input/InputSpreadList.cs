using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
	public class InputSpreadList<T> : SpreadList<T>
	{
		public InputSpreadList(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
		}
		
		//create a pin at position
		protected override ISpread<T> CreatePin(int pos)
		{
			//create pin name
			var origName = FAttribute.Name;
			FAttribute.Name = origName + " " + pos;
			
			var ret	= new InputWrapperPin<T>(FHost, FAttribute as InputAttribute).Pin;
			ret.PluginIO.Order = FOffsetCounter * 1000 + pos;
			
			//set attribute name back
			FAttribute.Name = origName;
			
			return ret;
		}
		
	}
}
