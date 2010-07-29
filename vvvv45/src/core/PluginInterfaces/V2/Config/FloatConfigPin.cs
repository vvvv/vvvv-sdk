using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public class FloatConfigPin : ValueConfigPin<float>
	{
		public FloatConfigPin(IPluginHost host, ConfigAttribute attribute)
			:base(host, attribute)
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
