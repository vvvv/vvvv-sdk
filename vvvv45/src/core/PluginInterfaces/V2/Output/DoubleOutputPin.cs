using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
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
				return FData[index % FData.Length];
			}
			set 
			{
				FData[index % FData.Length] = value;
			}
		}
	}
}
