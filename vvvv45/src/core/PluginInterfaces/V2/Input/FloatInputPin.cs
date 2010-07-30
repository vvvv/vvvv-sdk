using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
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
				return (float) FData[index % FData.Length];
			}
			set 
			{
				if (!FValueFastIn.IsConnected)
					FData[index % FData.Length] = (double) value;
			}
		}
	}
}
