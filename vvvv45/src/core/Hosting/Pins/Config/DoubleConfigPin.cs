using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
    [ComVisible(false)]
	public class DoubleConfigPin : ValueConfigPin<double>
	{
		public DoubleConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, double.MinValue, double.MaxValue, 0.01)
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
