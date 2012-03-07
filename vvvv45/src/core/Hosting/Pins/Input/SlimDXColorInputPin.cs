using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using SlimDX;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class SlimDXColorInputPin : DiffPin<Color4>
	{
		protected IColorIn FColorIn;

        public SlimDXColorInputPin(IPluginHost host, InputAttribute attribute)
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

        unsafe protected static void CopyToBuffer(Color4[] buffer, double* source, int length)
		{
            fixed (Color4* destination = buffer)
			{
                Color4* dst = destination;
				double* src = source;
				
				for (int i = 0; i < length / 4; i++)
				{
                    dst->Red = (float)*src;
					src++;
                    dst->Green = (float)*src;
					src++;
                    dst->Blue = (float)*src;
					src++;
                    dst->Alpha = (float)*src;
					src++;
					dst++;
				}
			}
		}
	}
}

