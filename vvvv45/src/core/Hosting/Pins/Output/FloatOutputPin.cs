using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class FloatOutputPin : ValueOutputPin<float>
	{
		public FloatOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyFromBuffer(float[] buffer, double* destination, int length)
		{
			for (int i = 0; i < length; i++)
				destination[i] = (double) buffer[i];
		}
	}
}
