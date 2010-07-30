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
				return (int) FData[index % FData.Length];
			}
			set 
			{
				if (!FValueFastIn.IsConnected)
					FData[index % FData.Length] = (double) value;
			}
		}
	}
}
