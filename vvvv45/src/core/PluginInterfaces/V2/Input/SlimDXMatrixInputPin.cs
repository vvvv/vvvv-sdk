using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class SlimDXMatrixInputPin : ObservablePin<Matrix>, IPinUpdater
	{
		protected ITransformIn FTransformIn;
		protected int FSliceCount;
		protected float[] FData;
		
		public SlimDXMatrixInputPin(IPluginHost host, InputAttribute attribute)
		{
			host.CreateTransformInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FTransformIn);
			
			FTransformIn.SetPinUpdater(this);
			
			SliceCount = 1;
		}
		
		public override IPluginIO PluginIO
		{
			get
			{
				return FTransformIn;
			}
		}
		
		public override bool IsChanged
		{
			get
			{
				return FTransformIn.PinIsChanged;
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
					FData = new float[value * 16];
				
				FSliceCount = value;
			}
		}
		
		unsafe public override Matrix this[int index]
		{
			get
			{
				fixed (float* ptr = FData)
				{
					return ((Matrix*)ptr)[index % FSliceCount];
				}
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		
		unsafe public override void Update()
		{
			if (IsChanged)
			{
				int sliceCount;
				float* source;
				
				FTransformIn.GetMatrixPointer(out sliceCount, out source);
				SliceCount = sliceCount;
				Marshal.Copy(new IntPtr(source), FData, 0, FData.Length);
			}
			
			base.Update();
		}
	}
}
