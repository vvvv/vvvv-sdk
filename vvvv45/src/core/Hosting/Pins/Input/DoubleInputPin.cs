using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
	public class DoubleInputPin : ValueInputPin<double>
	{
		public DoubleInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override double this[int index]
		{
			get
			{
				return FData[index % FSliceCount];
			}
			set
			{
				FData[index % FSliceCount] = value;
			}
		}
	}
	
	public class DiffDoubleInputPin : DiffValueInputPin<double>
	{
		public DiffDoubleInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override double this[int index]
		{
			get
			{
				return FData[index % FSliceCount];
			}
			set
			{
				FData[index % FSliceCount] = value;
			}
		}
	}
}
