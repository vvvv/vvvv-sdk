using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Streams;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class OutputSpreadList<T> : SpreadList<T>, IOutputPin
	{
		public OutputSpreadList(IOFactory ioFactory, OutputAttribute attribute)
			: base(ioFactory, attribute)
		{
		}
		
		protected override IOAttribute CreateAttribute(int position)
		{
			return new OutputAttribute(FAttribute.Name + " " + position);
		}
	}
}
