using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
	public class DoubleConfigPin : ValueConfigPin<double>
	{
		public DoubleConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override double this[int index] 
		{
			get 
			{
				double value;
				FValueConfig.GetValue(index, out value);
				return value;
			}
			set 
			{
				FValueConfig.SetValue(index, value);
			}
		}
	}
}
