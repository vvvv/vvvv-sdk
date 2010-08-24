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
				return FData[index % FSliceCount] >= 0.5;
			}
			set 
			{
				FData[index % FSliceCount] = value ? 1 : 0;
			}
		}
	}
}
