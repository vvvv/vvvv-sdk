using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
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
			
			var underFlow = length % 4;
			if (underFlow != 0)
				SliceCount = length / 4 + 1;
			else
				SliceCount = length / 4;
			
			if (FSliceCount > 0)
				CopyToBuffer(FBuffer, source, length, underFlow);
		}
		
		unsafe protected void CopyToBuffer(RGBAColor[] buffer, double* source, int length, int underFlow)
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
				
				if (underFlow > 0)
				{
					int i = length - underFlow;
					dst->R = *(source + i++ % length);
					dst->G = *(source + i++ % length);
					dst->B = *(source + i++ % length);
					dst->A = *(source + i++ % length);
				}
			}
		}
	}
}
