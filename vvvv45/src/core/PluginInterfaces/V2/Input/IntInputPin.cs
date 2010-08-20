using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
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
				if (!FValueFastIn.IsConnected)
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
				if (!FValueIn.IsConnected)
					FData[index % FSliceCount] = (double) value;
			}
		}
	}
}
