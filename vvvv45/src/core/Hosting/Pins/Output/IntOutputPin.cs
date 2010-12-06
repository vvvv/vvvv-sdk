using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class IntOutputPin : ValueOutputPin<int>
	{
		public IntOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, int.MinValue, int.MaxValue, 1)
		{
		}
		
		unsafe protected override void CopyFromBuffer(int[] buffer, double* destination, int length)
		{
			for (int i = 0; i < length; i++)
				destination[i] = (double) buffer[i];
		}
	}
}
