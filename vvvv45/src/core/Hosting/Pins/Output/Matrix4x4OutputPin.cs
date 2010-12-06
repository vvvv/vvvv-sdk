
using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

namespace VVVV.Hosting.Pins.Output
{
	public class Matrix4x4OutputPin : Pin<Matrix4x4>, IPinUpdater
	{
		protected ITransformOut FTransformOut;
		
		public Matrix4x4OutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateTransformOutput(FName, FSliceMode, FVisibility, out FTransformOut);
			base.InitializeInternalPin(FTransformOut);
		}
		
		unsafe protected void CopyFromBuffer(Matrix4x4[] buffer, float* destination, int length)
		{
			fixed (Matrix4x4* source = buffer)
			{
				Matrix4x4* src = source;
				Matrix* dst = (Matrix*) destination;
				
				for (int i = 0; i < length; i++)
					*(dst++) = (*(src++)).ToSlimDXMatrix();
			}
		}
		
		unsafe public override void Update()
		{
			base.Update();
			
			if (FAttribute.SliceMode != SliceMode.Single)
				FTransformOut.SliceCount = SliceCount;
			
			float* destination;
			FTransformOut.GetMatrixPointer(out destination);
			
			if (FSliceCount > 0)
				CopyFromBuffer(FBuffer, destination, SliceCount);
		}
	}
}
