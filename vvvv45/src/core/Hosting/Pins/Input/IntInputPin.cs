using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

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
				return (int) FData[VMath.Zmod(index, FSliceCount)];
			}
			set
			{
				FData[VMath.Zmod(index, FSliceCount)] = (double) value;
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
				return (int) Math.Round(FData[VMath.Zmod(index, FSliceCount)]);
			}
			set
			{
				FData[VMath.Zmod(index, FSliceCount)] = (double) value;
			}
		}
	}
}
