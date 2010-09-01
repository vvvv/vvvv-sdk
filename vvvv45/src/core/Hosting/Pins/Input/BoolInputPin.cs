using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
	public class BoolInputPin : ValueInputPin<bool>
	{
		public BoolInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override bool this[int index]
		{
			get
			{
				return FData[index % FSliceCount] >= 0.5;
			}
			set
			{
				FData[index % FSliceCount] = value ? 1.0 : 0.0;
			}
		}
	}
	
	public class DiffBoolInputPin : DiffValueInputPin<bool>
	{
		public DiffBoolInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override bool this[int index]
		{
			get
			{
				return FData[index % FSliceCount] >= 0.5;
			}
			set
			{
				FData[index % FSliceCount] = value ? 1.0 : 0.0;
			}
		}
	}
}
