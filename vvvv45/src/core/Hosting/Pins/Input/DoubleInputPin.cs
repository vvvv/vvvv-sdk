using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

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
				return FData[VMath.Zmod(index, FSliceCount)];
			}
			set 
			{
				FData[VMath.Zmod(index, FSliceCount)] = value;
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
				return FData[VMath.Zmod(index, FSliceCount)];
			}
			set
			{
				FData[VMath.Zmod(index, FSliceCount)] = value;
			}
		}
	}
}
