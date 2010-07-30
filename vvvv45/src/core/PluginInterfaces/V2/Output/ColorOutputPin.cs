using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class ColorOutputPin : Pin<RGBAColor>
	{
		protected IColorOut FColorOut;
		
		public ColorOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			host.CreateColorOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FColorOut);
			FColorOut.SetSubType(new RGBAColor(attribute.DefaultValues), attribute.HasAlpha);
		}
		
		public override int SliceCount 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				FColorOut.SliceCount = value;
			}
		}
		
		public override RGBAColor this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				FColorOut.SetColor(index, value);
			}
		}
	}
}
