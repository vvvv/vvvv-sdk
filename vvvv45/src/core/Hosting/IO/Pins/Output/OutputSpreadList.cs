using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	class OutputSpreadList<T> : SpreadList<ISpread<T>>
	{
		public OutputSpreadList(IIOFactory ioFactory, OutputAttribute attribute)
			: base(ioFactory, attribute)
		{
		}
		
		protected override IOAttribute CreateAttribute(int position)
		{
			return new OutputAttribute(FAttribute.Name + " " + position);
		}
	}
}
