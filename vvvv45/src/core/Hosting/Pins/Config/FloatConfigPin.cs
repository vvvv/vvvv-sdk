using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
    [ComVisible(false)]
	public class FloatConfigPin : ValueConfigPin<float>
	{
		public FloatConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		public override float this[int index] 
		{
			get 
			{
				double value;
				FValueConfig.GetValue(index, out value);
				return (float) value;
			}
			set 
			{
				FValueConfig.SetValue(index, (double) value);
			}
		}
	}
}
