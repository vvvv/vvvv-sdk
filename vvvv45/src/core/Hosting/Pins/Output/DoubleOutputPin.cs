using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class DoubleOutputPin : ValueOutputPin<double>
	{
		public DoubleOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override double this[int index] 
		{
			get 
			{
				return FData[VMath.Zmod(index, FSliceCount)];
			}
			set 
			{
				FData[VMath.Zmod(index, FSliceCount)] = value;
			}
		}
	}
}
