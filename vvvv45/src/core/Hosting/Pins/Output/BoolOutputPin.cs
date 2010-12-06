using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class BoolOutputPin : ValueOutputPin<bool>
	{
		public BoolOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, 0, 1, 1)
		{
		}
		
		unsafe protected override void CopyFromBuffer(bool[] buffer, double* destination, int length)
		{
			for (int i = 0; i < length; i++)
				destination[i] = buffer[i] ? 1.0 : 0.0;
		}
	}
}
