using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class DiffInputSpreadList<T> : DiffSpreadList<T>, IInputPin
	{
		public DiffInputSpreadList(IOFactory ioFactory, InputAttribute attribute)
			: base(ioFactory, attribute)
		{
		}
		
		//create a pin at position
		protected override IDiffSpread<T> CreateDiffSpread(int pos)
		{
			var attribute = new InputAttribute(FAttribute.Name + " " + pos);
			attribute.IsPinGroup = false;
			attribute.Order = FAttribute.Order + FOffsetCounter * 1000 + pos;
			
			return FIOFactory.CreateIO<IDiffSpread<T>>(attribute);
		}
		
		
		public override bool Sync()
		{
			// TODO
			return base.Sync();
		}
	}
}
