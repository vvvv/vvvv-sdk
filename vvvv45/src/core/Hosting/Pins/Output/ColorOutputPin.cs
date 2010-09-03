using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class ColorOutputPin : Pin<RGBAColor>, IPinUpdater
	{
		protected IColorOut FColorOut;
		protected new double[] FData;
		
		public ColorOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateColorOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FColorOut);
			FColorOut.SetSubType(new RGBAColor(attribute.DefaultValues), attribute.HasAlpha);
			
			base.Initialize(FColorOut);
		}
		
		public override int SliceCount
		{
			get
			{
				return FSliceCount;
			}
			set
			{
				if (FSliceCount != value)
				{
					FData = new double[value * 4];
					
					FSliceCount = value;
					
					if (FAttribute.SliceMode != SliceMode.Single)
						FColorOut.SliceCount = value;
				}
			}
		}
		
		unsafe public override RGBAColor this[int index]
		{
			get
			{
				fixed (double* ptr = FData)
				{
					return ((RGBAColor*)ptr)[VMath.Zmod(index, FSliceCount)];
				}
			}
			set
			{
				fixed (double* ptr = FData)
				{
					((RGBAColor*)ptr)[VMath.Zmod(index, FSliceCount)] = value;
				}
			}
		}
		
		unsafe public override void Update()
		{
			base.Update();
			
			double* destination;
			FColorOut.GetColorPointer(out destination);
			
			if (FSliceCount > 0)
				Marshal.Copy(FData, 0, new IntPtr(destination), FData.Length);
		}
	}
}
