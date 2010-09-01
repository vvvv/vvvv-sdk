using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
	public class IntInputPin : ValueInputPin<int>
	{
		public IntInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override int this[int index]
		{
			get
			{
				return (int) FData[index % FSliceCount];
			}
			set
			{
				FData[index % FSliceCount] = (double) value;
			}
		}
	}
	
	public class DiffIntInputPin : DiffValueInputPin<int>
	{
		public DiffIntInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override int this[int index]
		{
			get
			{
				return (int) FData[index % FSliceCount];
			}
			set
			{
				FData[index % FSliceCount] = (double) value;
			}
		}
	}
}
