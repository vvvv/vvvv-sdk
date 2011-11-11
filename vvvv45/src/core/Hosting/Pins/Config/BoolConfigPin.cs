using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
    [ComVisible(false)]
	public class BoolConfigPin : ValueConfigPin<bool>
	{
		public BoolConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, 0, 1, 1)
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
				FValueConfig.SetValue(index, value ? 1.0 : 0.0);
			}
		}
	}
}
