using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class IntOutputPin : ValueOutputPin<int>
	{
		public IntOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, int.MinValue, int.MaxValue, 1)
		{
		}
		
		unsafe protected override void CopyFromBuffer(int[] buffer, double* destination, int startIndex, int length)
		{
			for (int i = startIndex; i < startIndex + length; i++)
				destination[i] = (double) buffer[i];
		}
	}
}
