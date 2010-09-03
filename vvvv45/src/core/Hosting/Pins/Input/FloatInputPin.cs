using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class FloatInputPin : ValueInputPin<float>
	{
		public FloatInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override float this[int index]
		{
			get
			{
				return (float) FData[VMath.Zmod(index, FSliceCount)];
			}
			set
			{
				FData[VMath.Zmod(index, FSliceCount)] = (double) value;
			}
		}
	}
	
	public class DiffFloatInputPin : DiffValueInputPin<float>
	{
		public DiffFloatInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override float this[int index]
		{
			get
			{
				return (float) FData[VMath.Zmod(index, FSliceCount)];
			}
			set
			{
				FData[VMath.Zmod(index, FSliceCount)] = (double) value;
			}
		}
	}
}
