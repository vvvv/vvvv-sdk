using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class IntOutputPin : ValueOutputPin<int>
	{
		public IntOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override int this[int index] 
		{
			get 
			{
				return (int)FData[VMath.Zmod(index, FSliceCount)];
			}
			set 
			{
				FData[VMath.Zmod(index, FSliceCount)] = value;
			}
		}
	}
}
