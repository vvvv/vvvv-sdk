using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class OutputSpreadList<T> : SpreadList<T>
	{
		public OutputSpreadList(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
		}
		
		//create a pin at position
		protected override ISpread<T> CreateSpread(int pos)
		{
			var attribute = new OutputAttribute(FAttribute.Name + " " + pos);
			attribute.IsPinGroup = false;
			attribute.Order = FAttribute.Order + FOffsetCounter * 1000 + pos;
			
			return PinFactory.CreateSpread<T>(FHost, attribute);
		}

	}
}
