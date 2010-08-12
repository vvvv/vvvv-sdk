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
				throw new NotImplementedException();
			}
			set 
			{
				FData[index] = (double) value;
			}
		}
	}
}
