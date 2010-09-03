using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class BoolOutputPin : ValueOutputPin<bool>
	{
		public BoolOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override bool this[int index] 
		{
			get 
			{
				return FData[VMath.Zmod(index, FSliceCount)] >= 0.5;
			}
			set
			{
				FData[VMath.Zmod(index, FSliceCount)] = value ? 1 : 0;
			}
		}
	}
}
