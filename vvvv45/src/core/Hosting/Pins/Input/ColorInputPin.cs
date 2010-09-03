using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class ColorInputPin : DiffPin<RGBAColor>, IPinUpdater
	{
		protected IColorIn FColorIn;
		new protected double[] FData;
		
		public ColorInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateColorInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FColorIn);
			FColorIn.SetSubType(new RGBAColor(attribute.DefaultColor), attribute.HasAlpha);
			
			base.Initialize(FColorIn);
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
					return ((RGBAColor*)ptr)[VMath.Zmod(index, FSliceCount)];
				}
			}
			set
			{
				if (!FColorIn.IsConnected)
				{
					fixed (double* ptr = FData)
					{
						((RGBAColor*)ptr)[VMath.Zmod(index, FSliceCount)] = value;
					}
				}
			}
		}
		
		unsafe public override void Update()
		{
			if (IsChanged)
			{
				int sliceCount;
				double* source;
				
				FColorIn.GetColorPointer(out sliceCount, out source);
				SliceCount = sliceCount;
				
				if (FSliceCount > 0)
					Marshal.Copy(new IntPtr(source), FData, 0, FData.Length);
			}
			
			base.Update();
		}
	}
}
