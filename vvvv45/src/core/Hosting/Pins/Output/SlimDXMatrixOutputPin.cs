using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class SlimDXMatrixOutputPin : Pin<Matrix>
	{
		protected ITransformOut FTransformOut;
		
		public SlimDXMatrixOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateTransformOutput(FName, FSliceMode, FVisibility, out FTransformOut);
			base.InitializeInternalPin(FTransformOut);
		}
		
		unsafe protected static void CopyFromBuffer(Matrix[] buffer, float* destination, int length)
		{
			fixed (Matrix* source = buffer)
			{
				Matrix* src = source;
				Matrix* dst = (Matrix*) destination;
				
				for (int i = 0; i < length; i++)
					*(dst++) = *(src++);
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
