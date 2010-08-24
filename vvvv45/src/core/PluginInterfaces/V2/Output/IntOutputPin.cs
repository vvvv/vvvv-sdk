using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
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
				return (int)FData[index % FSliceCount];
			}
			set 
			{
				FData[index % FSliceCount] = value;
			}
		}
	}
}
