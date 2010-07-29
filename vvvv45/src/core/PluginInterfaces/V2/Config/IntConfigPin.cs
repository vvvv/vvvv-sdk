using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public class IntConfigPin : ValueConfigPin<int>
	{
		public IntConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override int this[int index] 
		{
			get 
			{
				double value;
				FValueConfig.GetValue(index, out value);
				return (int) value;
			}
			set 
			{
				FValueConfig.SetValue(index, (double) value);
			}
		}
	}
}
