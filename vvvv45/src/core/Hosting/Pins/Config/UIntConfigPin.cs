using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Config
{
    [ComVisible(false)]
	public class UIntConfigPin : ValueConfigPin<uint>
	{
		public UIntConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute, uint.MinValue, uint.MaxValue, 1)
		{
		}
		
		public override uint this[int index] 
		{
			get 
			{
				double value;
				FValueConfig.GetValue(index, out value);
				return (uint) value;
			}
			set 
			{
				FValueConfig.SetValue(index, (double) value);
			}
		}
	}
}
