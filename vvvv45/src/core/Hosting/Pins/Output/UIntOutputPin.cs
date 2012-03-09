using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class UIntOutputPin : ValueOutputPin<uint>
	{
		public UIntOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, uint.MinValue, uint.MaxValue, 1)
		{
		}
		
		unsafe protected override void CopyFromBuffer(uint[] buffer, double* destination, int startIndex, int length)
		{
			for (int i = startIndex; i < startIndex + length; i++)
				destination[i] = (double) buffer[i];
		}
	}
}
