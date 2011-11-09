using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class SlimDXMatrixInputPin : DiffPin<Matrix>, IPinUpdater
	{
		protected ITransformIn FTransformIn;
		
		public SlimDXMatrixInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateTransformInput(FName, FSliceMode, FVisibility, out FTransformIn);
			base.InitializeInternalPin(FTransformIn);
		}
		
		protected override bool IsInternalPinChanged
		{
			get
			{
				return FTransformIn.PinIsChanged;
			}
		}
		
		unsafe protected static void CopyToBuffer(Matrix[] buffer, float* source, int length)
		{
			fixed (Matrix* destination = buffer)
			{
				Matrix* dst = destination;
				Matrix* src = (Matrix*) source;
				
				for (int i = 0; i < length; i++)
					*(dst++) = *(src++);
			}
		}
		
		unsafe protected override void DoUpdate()
		{
			int length;
			float* source;
			
			FTransformIn.GetMatrixPointer(out length, out source);
			SliceCount = length;
			
			if (FSliceCount > 0)
				CopyToBuffer(FBuffer, source, length);
		}
	}
}
