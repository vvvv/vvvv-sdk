using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class SlimDXMatrixInputPin : DiffPin<Matrix>, IPinUpdater
	{
		protected ITransformIn FTransformIn;
		protected new float[] FData;
		
		public SlimDXMatrixInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateTransformInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FTransformIn);
			
			base.Initialize(FTransformIn);
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
				{
					FData = new float[value * 16];
					
					FSliceCount = value;
				}
			}
		}
		
		unsafe public override Matrix this[int index]
		{
			get
			{
				fixed (float* ptr = FData)
				{
					return ((Matrix*)ptr)[VMath.Zmod(index, FSliceCount)];
				}
			}
			set
			{
				fixed (float* ptr = FData)
				{
					((Matrix*)ptr)[VMath.Zmod(index, FSliceCount)] = value;
				}
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
				
				if (FSliceCount > 0)
					Marshal.Copy(new IntPtr(source), FData, 0, FData.Length);
			}
			
			base.Update();
		}
	}
}
