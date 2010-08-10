using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class BoolInputPin : ValueInputPin<bool>
	{
		public BoolInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override bool this[int index] 
		{
			get 
			{
				return FData[index] >= 0.5;
			}
			set 
			{
				if (!FValueFastIn.IsConnected)
					FData[index] = value ? 1.0 : 0.0;
			}
		}
	}
	
	public class ObservableBoolInputPin : ObservableValueInputPin<bool>
	{
		public ObservableBoolInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override bool this[int index] 
		{
			get 
			{
				return FData[index] >= 0.5;
			}
			set 
			{
				if (!FValueIn.IsConnected)
					FData[index] = value ? 1.0 : 0.0;
			}
		}
	}
}
