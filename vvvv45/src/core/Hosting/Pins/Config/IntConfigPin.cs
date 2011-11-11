using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
    [ComVisible(false)]
	public class IntConfigPin : ValueConfigPin<int>
	{
		public IntConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, int.MinValue, int.MaxValue, 1)
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
