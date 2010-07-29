using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace VVVV.PluginInterfaces.V2
{
	public class ColorInputPin : InputPin<RGBAColor>
	{
		protected IColorIn FColorIn;
		
		public ColorInputPin(IPluginHost host, InputAttribute attribute)
		{
			host.CreateColorInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FColorIn);
			FColorIn.SetSubType(new RGBAColor(attribute.DefaultColor), attribute.HasAlpha);
		}
		
		public override IPluginIn PluginIn 
		{
			get 
			{
				return FColorIn;
			}
		}
		
		public override RGBAColor this[int index] 
		{
			get 
			{
				RGBAColor value;
				FColorIn.GetColor(index, out value);
				return value;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
	}
}
