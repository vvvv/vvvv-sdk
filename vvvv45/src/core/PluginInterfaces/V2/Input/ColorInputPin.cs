using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class ColorInputPin : ObservablePin<RGBAColor>
	{
		protected IColorIn FColorIn;
		
		public ColorInputPin(IPluginHost host, InputAttribute attribute)
		{
			host.CreateColorInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FColorIn);
			FColorIn.SetSubType(new RGBAColor(attribute.DefaultValues), attribute.HasAlpha);
		}
		
		public override int SliceCount 
		{
			get 
			{
				return FColorIn.SliceCount;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
		
		public override bool IsChanged 
		{
			get 
			{
				return FColorIn.PinIsChanged;
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
