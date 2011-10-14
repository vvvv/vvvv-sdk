using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class DiffInputSpreadList<T> : DiffSpreadList<T>
	{
		public DiffInputSpreadList(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
		}
		
		//create a pin at position
		protected override IDiffSpread<T> CreateDiffSpread(int pos)
		{
			var attribute = new InputAttribute(FAttribute.Name + " " + pos);
			attribute.IsPinGroup = false;
			attribute.Order = FAttribute.Order + FOffsetCounter * 1000 + pos;
			
			return PinFactory.CreateDiffSpread<T>(FHost, attribute);
		}
		
	}
}
