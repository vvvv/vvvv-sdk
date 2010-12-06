using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class FloatInputPin : ValueInputPin<float>
	{
		public FloatInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(float[] buffer, double* source, int startingIndex, int length)
		{
			fixed (float* destination = buffer)
			{
				float* dst = destination + startingIndex;
				double* src = source + startingIndex;
				
				for (int i = 0; i < length; i++)
					*(dst++) = (float) *(src++);
			}
		}
	}
	
	public class DiffFloatInputPin : DiffValueInputPin<float>
	{
		public DiffFloatInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(float[] buffer, double* source, int startingIndex, int length)
		{
			fixed (float* destination = buffer)
			{
				float* dst = destination + startingIndex;
				double* src = source + startingIndex;
				
				for (int i = 0; i < length; i++)
					*(dst++) = (float) *(src++);
			}
		}
	}
}
