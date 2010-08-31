using System;
using System.Runtime.InteropServices;
using SlimDX;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
	public class SlimDXMatrixOutputPin : Pin<Matrix>, IPinUpdater
	{
		protected ITransformOut FTransformOut;
		protected new float[] FData;
		
		public SlimDXMatrixOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateTransformOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FTransformOut);
			
			base.Initialize(FTransformOut);
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
					
					if (FAttribute.SliceMode != SliceMode.Single)
						FTransformOut.SliceCount = value;
				}
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
				fixed (float* ptr = FData)
				{
					((Matrix*)ptr)[index % FSliceCount] = value;
				}
			}
		}
		
		unsafe public override void Update()
		{
			float* destination;
			FTransformOut.GetMatrixPointer(out destination);
			
			if (FSliceCount > 0)
				Marshal.Copy(FData, 0, new IntPtr(destination), FData.Length);
			
			base.Update();
		}
	}
}
