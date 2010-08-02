
using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class Matrix4x4OutputPin : Pin<Matrix4x4>, IPinUpdater
	{
		protected ITransformOut FTransformOut;
		protected float[] FData;
		protected int FSliceCount;
		
		public Matrix4x4OutputPin(IPluginHost host, OutputAttribute attribute)
		{
			host.CreateTransformOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FTransformOut);
			
			FTransformOut.SetPinUpdater(this);
			
			FData = new float[16];
		}
		
		public override int SliceCount 
		{
			get 
			{
				return FSliceCount;
			}
			set 
			{
				if (FData.Length != value)
					FData = new float[value * 16];
				
				FSliceCount = value;
				FTransformOut.SliceCount = value;
			}
		}
		
		unsafe public override Matrix4x4 this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				fixed (float* ptr = FData)
				{
					((Matrix*)ptr)[index % FSliceCount] = value.ToSlimDXMatrix();
				}
			}
		}
		
		unsafe public override void Update()
		{
			float* destination;
			FTransformOut.GetMatrixPointer(out destination);
			
			Marshal.Copy(FData, 0, new IntPtr(destination), FData.Length);
		}
	}
}
