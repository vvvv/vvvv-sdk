using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace VVVV.PluginInterfaces.V2
{
	public class ColorConfigPin : ConfigPin<RGBAColor>
	{
		protected IColorConfig FColorConfig;
		
		public ColorConfigPin(IPluginHost host, ConfigAttribute attribute)
		{
			host.CreateColorConfig(attribute.Name, attribute.SliceMode, attribute.Visibility, out FColorConfig);
			FColorConfig.SetSubType(new RGBAColor(attribute.DefaultColor), attribute.HasAlpha);
		}
		
		public override IPluginConfig PluginConfig
		{
			get 
			{
				return FColorConfig;
			}
		}
		
		public override RGBAColor this[int index] 
		{
			get 
			{
				RGBAColor value;
				FColorConfig.GetColor(index, out value);
				return value;
			}
			set 
			{
				FColorConfig.SetColor(index, value);
			}
		}
	}
}
