using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Output
{
	public class DoubleOutputPin : ValueOutputPin<double>
	{
		public DoubleOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyFromBuffer(double[] buffer, double* destination, int length)
		{
			Marshal.Copy(buffer, 0, (IntPtr) destination, length);
		}
	}
}
