using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	class OutputSpreadList<T> : SpreadList<T>
	{
		public OutputSpreadList(IIOFactory ioFactory, OutputAttribute attribute)
			: base(ioFactory, attribute)
		{
		}
		
		//create a pin at position
		protected override ISpread<T> CreateSpread(int pos)
		{
			var attribute = new OutputAttribute(FAttribute.Name + " " + pos);
			attribute.IsPinGroup = false;
			attribute.Order = FAttribute.Order + FOffsetCounter * 1000 + pos;
			
			return FIOFactory.CreateIO<ISpread<T>>(attribute);
		}

		
		public override void Flush()
		{
			base.Flush();
			// TODO
		}
	}
}
