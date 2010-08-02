
using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class Matrix4x4InputPin : ObservablePin<Matrix4x4>, IPinUpdater
	{
		protected ITransformIn FTransformIn;
		protected int FSliceCount = 1;
		protected float[] FData; 
		
		public Matrix4x4InputPin(IPluginHost host, InputAttribute attribute)
		{
			host.CreateTransformInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FTransformIn);
			
			FTransformIn.SetPinUpdater(this);
			
			FData = new float[16];
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
		
		unsafe public override Matrix4x4 this[int index] 
		{
			get 
			{
				fixed (float* ptr = FData)
				{
					return ((Matrix*)ptr)[index % FSliceCount].ToMatrix4x4();
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
			float* source;
			
			FTransformIn.GetMatrixPointer(out sliceCount, out source);
			SliceCount = sliceCount;
			Marshal.Copy(new IntPtr(source), FData, 0, FData.Length);
		}
	}
}
