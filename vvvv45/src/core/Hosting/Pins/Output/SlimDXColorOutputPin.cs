using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using SlimDX;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class SlimDXColorOutputPin : Pin<Color4>
	{
		protected IColorOut FColorOut;

        public SlimDXColorOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateColorOutput(FName, FSliceMode, FVisibility, out FColorOut);
			FColorOut.SetSubType(new RGBAColor(attribute.DefaultValues), attribute.HasAlpha);
			base.InitializeInternalPin(FColorOut);
		}
		
		unsafe public override void Update()
		{
			base.Update();
			
			if (FAttribute.SliceMode != SliceMode.Single)
				FColorOut.SliceCount = FSliceCount;
			
			if (FSliceCount > 0)
			{
				double* dst;
				FColorOut.GetColorPointer(out dst);
				CopyFromBuffer(FBuffer, dst, FSliceCount * 4);
			}
		}

        unsafe protected static void CopyFromBuffer(Color4[] buffer, double* dst, int length)
		{
            fixed (Color4* source = buffer)
			{
                Color4* src = source;
				for (int i = 0; i < length / 4; i++)
				{
					*dst = src->Red;
					dst++;
					*dst = src->Green;
					dst++;
					*dst = src->Blue;
					dst++;
					*dst = src->Alpha;
					dst++;
					src++;
				}
			}
		}
	}
}

