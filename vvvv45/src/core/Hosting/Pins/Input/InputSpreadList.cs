using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class InputSpreadList<T> : SpreadList<T>, IInputPin
	{
		public InputSpreadList(IOFactory ioFactory, InputAttribute attribute)
			: base(ioFactory, attribute)
		{
		}

		//create a pin at position
		protected override IOAttribute CreateAttribute(int position)
		{
			return new InputAttribute(FAttribute.Name + " " + position)
			{
				IsPinGroup = false,
				AutoValidate = false
			};
		}
	}
}
