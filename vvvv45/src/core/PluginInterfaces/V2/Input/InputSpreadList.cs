using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class InputSpreadList<T, TSub> : SpreadList<T, TSub>
	{
		public InputSpreadList(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
		}
		
		//create a pin at position
		protected override T CreatePin(int pos)
		{
			//create pin name
			var origName = FAttribute.Name;
			FAttribute.Name = origName + " " + pos;
			
			var ret	= new InputWrapperPin<TSub>(FHost, FAttribute as InputAttribute).Pin;
			
			//set attribute name back
			FAttribute.Name = origName;
			
			return (T)(object)ret;
		}
		
	}
}
