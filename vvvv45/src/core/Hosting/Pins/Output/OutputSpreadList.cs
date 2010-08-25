using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
	public class OutputSpreadList<T> : SpreadList<T>
	{
		public OutputSpreadList(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
		}
		
		//create a pin at position
		protected override ISpread<T> CreatePin(int pos)
		{
			//create pin name
			var origName = FAttribute.Name;
			FAttribute.Name = origName + " " + pos;
			
			var ret	= new OutputWrapperPin<T>(FHost, FAttribute as OutputAttribute).Pin;
			ret.PluginIO.Order = FOffsetCounter * 1000 + pos;
			
			//set attribute name back
			FAttribute.Name = origName;
			
			return ret;
		}

	}
}
