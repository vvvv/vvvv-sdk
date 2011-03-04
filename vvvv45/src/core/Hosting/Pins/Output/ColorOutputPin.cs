using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class ColorOutputPin : Pin<RGBAColor>
	{
		protected IColorOut FColorOut;
		
		public ColorOutputPin(IPluginHost host, OutputAttribute attribute)
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
		
		unsafe protected void CopyFromBuffer(RGBAColor[] buffer, double* dst, int length)
		{
			fixed (RGBAColor* source = buffer)
			{
				RGBAColor* src = source;
				for (int i = 0; i < length / 4; i++)
				{
					*dst = src->R;
					dst++;
					*dst = src->G;
					dst++;
					*dst = src->B;
					dst++;
					*dst = src->A;
					dst++;
					src++;
				}
			}
		}
	}
}
