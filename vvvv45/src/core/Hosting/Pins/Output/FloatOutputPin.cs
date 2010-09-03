using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class FloatOutputPin : ValueOutputPin<float>
	{
		public FloatOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override float this[int index] 
		{
			get 
			{
				return (float)FData[VMath.Zmod(index, FSliceCount)];
			}
			set 
			{
				FData[VMath.Zmod(index, FSliceCount)] = value;
			}
		}
	}
}
