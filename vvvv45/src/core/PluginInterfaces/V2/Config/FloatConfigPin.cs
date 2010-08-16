using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Config
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
				return (float)FData[index % FSliceCount];
			}
			set 
			{
				FData[index % FSliceCount] = value;
			}
		}
	}
}
