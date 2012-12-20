using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	class InputSpreadList<T> : SpreadList<ISpread<T>>
	{
		public InputSpreadList(IIOFactory ioFactory, InputAttribute attribute)
			: base(ioFactory, attribute)
		{
		}

		//create a pin at position
		protected override IOAttribute CreateAttribute(int position)
		{
			return new InputAttribute(FAttribute.Name + " " + position)
			{
				IsPinGroup = false
			};
		}
	}
}
