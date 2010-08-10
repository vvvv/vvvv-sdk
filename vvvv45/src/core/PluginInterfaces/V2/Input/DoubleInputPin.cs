using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
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
				return FData[index % FData.Length];
			}
			set 
			{
				if (!FValueFastIn.IsConnected)
					FData[index % FData.Length] = value;
			}
		}
	}
	
	public class ObservableDoubleInputPin : ObservableValueInputPin<double>
	{
		public ObservableDoubleInputPin(IPluginHost host, InputAttribute attribute)
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
				if (!FValueIn.IsConnected)
					FData[index % FData.Length] = value;
			}
		}
	}
}
