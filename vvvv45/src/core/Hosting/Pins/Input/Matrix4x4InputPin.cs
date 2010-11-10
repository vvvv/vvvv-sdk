
using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

namespace VVVV.Hosting.Pins.Input
{
	public class Matrix4x4InputPin : DiffPin<Matrix4x4>, IPinUpdater
	{
		protected ITransformIn FTransformIn;
		protected new float[] FData;
		
		public Matrix4x4InputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateTransformInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FTransformIn);
			
			FData = new float[16];
			
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
		
		unsafe public override Matrix4x4 this[int index]
		{
			get
			{
				fixed (float* ptr = FData)
				{
					return ((Matrix*)ptr)[VMath.Zmod(index, FSliceCount)].ToMatrix4x4();
				}
			}
			set
			{
				fixed (float* ptr = FData)
				{
					((Matrix*)ptr)[VMath.Zmod(index, FSliceCount)] = value.ToSlimDXMatrix();
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
