using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;

namespace VVVV.Hosting.Pins.Config
{
	public class ColorConfigPin : ConfigPin<RGBAColor>
	{
		protected IColorConfig FColorConfig;
		
		public ColorConfigPin(IPluginHost host, ConfigAttribute attribute)
			: base(host, attribute)
		{
			host.CreateColorConfig(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FColorConfig);
			FColorConfig.SetSubType(new RGBAColor(attribute.DefaultValues), attribute.HasAlpha);
			FColorConfig.SetPinUpdater(this);
		}
		
		protected override IPluginConfig PluginConfig 
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
