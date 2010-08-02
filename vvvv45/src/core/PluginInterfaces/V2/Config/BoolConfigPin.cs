using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
{
	public class BoolConfigPin : ValueConfigPin<bool>
	{
		public BoolConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override bool this[int index] 
		{
			get 
			{
				double value;
				FValueConfig.GetValue(index, out value);
				return value >= 0.5;
			}
			set
			{
				FValueConfig.SetValue(index, value ? 1 : 0);
			}
		}
	}
}
