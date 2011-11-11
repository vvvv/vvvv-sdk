using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class ColorInputPin : DiffPin<RGBAColor>
	{
		protected IColorIn FColorIn;
		
		public ColorInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateColorInput(FName, FSliceMode, FVisibility, out FColorIn);
			FColorIn.SetSubType(new RGBAColor(attribute.DefaultColor), attribute.HasAlpha);
			base.InitializeInternalPin(FColorIn);
		}
		
		protected override bool IsInternalPinChanged
		{
			get
			{
				return FColorIn.PinIsChanged;
			}
		}
		
		unsafe protected override void DoUpdate()
		{
			int length;
			double* source;
			
			FColorIn.GetColorPointer(out length, out source);
			
			SliceCount = length;
			
			if (FSliceCount > 0)
				CopyToBuffer(FBuffer, source, length * 4);
		}
		
		unsafe protected static void CopyToBuffer(RGBAColor[] buffer, double* source, int length)
		{
			fixed (RGBAColor* destination = buffer)
			{
				RGBAColor* dst = destination;
				double* src = source;
				
				for (int i = 0; i < length / 4; i++)
				{
					dst->R = *src;
					src++;
					dst->G = *src;
					src++;
					dst->B = *src;
					src++;
					dst->A = *src;
					src++;
					dst++;
				}
			}
		}
	}
}
