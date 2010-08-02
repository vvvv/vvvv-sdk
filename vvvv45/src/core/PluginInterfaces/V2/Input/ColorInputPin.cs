using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class ColorInputPin : ObservablePin<RGBAColor>, IPinUpdater
	{
		protected IColorIn FColorIn;
		protected int FSliceCount = 1;
		protected double[] FData; 
		
		public ColorInputPin(IPluginHost host, InputAttribute attribute)
		{
			host.CreateColorInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FColorIn);
			FColorIn.SetSubType(new RGBAColor(attribute.DefaultValues), attribute.HasAlpha);
			FColorIn.SetPinUpdater(this);
			
			FData = new double[4];
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
			}
		}
		
		public override bool IsChanged 
		{
			get 
			{
				return FColorIn.PinIsChanged;
			}
		}
		
		unsafe public override RGBAColor this[int index] 
		{
			get 
			{
				fixed (double* ptr = FData)
				{
					return ((RGBAColor*)ptr)[index % FSliceCount];
				}
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
		
		unsafe public override void Update()
		{
			int sliceCount;
			double* source;
			
			FColorIn.GetColorPointer(out sliceCount, out source);
			SliceCount = sliceCount;
			Marshal.Copy(new IntPtr(source), FData, 0, FData.Length);
		}
	}
}
