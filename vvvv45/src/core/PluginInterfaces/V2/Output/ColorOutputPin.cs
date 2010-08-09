using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class ColorOutputPin : Pin<RGBAColor>, IPinUpdater
	{
		protected IColorOut FColorOut;
		protected double[] FData;
		protected int FSliceCount;
		
		public ColorOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			host.CreateColorOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FColorOut);
			FColorOut.SetSubType(new RGBAColor(attribute.DefaultValues), attribute.HasAlpha);
			FColorOut.SetPinUpdater(this);
			
			SliceCount = 1;
		}
		
		public override IPluginIO PluginIO 
		{
			get
			{
				return FColorOut;
			}
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
					FData = new double[value * 4];
				
				FSliceCount = value;
				FColorOut.SliceCount = value;
			}
		}
		
		unsafe public override RGBAColor this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				fixed (double* ptr = FData)
				{
					((RGBAColor*)ptr)[index % FSliceCount] = value;
				}
			}
		}
		
		unsafe public override void Update()
		{
			double* destination;
			FColorOut.GetColorPointer(out destination);
			
			Marshal.Copy(FData, 0, new IntPtr(destination), FData.Length);
		}
	}
}
