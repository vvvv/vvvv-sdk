using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
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
				FValueOut.SetValue(index, (double) value);
			}
		}
	}
}
