using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class FloatOutputPin : ValueOutputPin<float>
	{
		public FloatOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, float.MinValue, float.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyFromBuffer(float[] buffer, double* destination, int startIndex, int length)
		{
			for (int i = startIndex; i < startIndex + length; i++)
				destination[i] = (double) buffer[i];
		}
	}
}
