using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	class InputSpreadList<T> : SpreadList<T>
	{
		public InputSpreadList(IIOFactory ioFactory, InputAttribute attribute)
			: base(ioFactory, attribute)
		{
		}

		//create a pin at position
		protected override ISpread<T> CreateSpread(int pos)
		{
			var attribute = new InputAttribute(FAttribute.Name + " " + pos);
			attribute.IsPinGroup = false;
			attribute.Order = FAttribute.Order + FOffsetCounter * 1000 + pos;
			
			return FIOFactory.CreateIO<ISpread<T>>(attribute);
		}
	}
}
