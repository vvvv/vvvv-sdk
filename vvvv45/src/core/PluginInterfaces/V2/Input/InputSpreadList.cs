using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
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
			
			//set attribute name back
			FAttribute.Name = origName;
			
			return ret;
		}
		
	}
}
