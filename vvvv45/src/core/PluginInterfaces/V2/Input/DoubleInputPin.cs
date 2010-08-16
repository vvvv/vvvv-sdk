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
				return FData[index % FSliceCount];
			}
			set 
			{
				if (!FValueFastIn.IsConnected)
					FData[index % FSliceCount] = value;
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
				return FData[index % FSliceCount];
			}
			set 
			{
				if (!FValueIn.IsConnected)
					FData[index % FSliceCount] = value;
			}
		}
	}
}
