using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class DoubleInputPin : ValueInputPin<double>
	{
		public DoubleInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(double[] buffer, double* source, int startingIndex, int length)
		{
			if (startingIndex == 0)
				Marshal.Copy(new IntPtr(source), buffer, 0, length);
			else
			{
				fixed (double* destination = buffer)
				{
					double* dst = destination + startingIndex;
					double* src = source + startingIndex;
					
					for (int i = 0; i < length; i++)
						*(dst++) = *(src++);
				}
			}
		}
	}
	
	[ComVisible(false)]
	public class DiffDoubleInputPin : DiffValueInputPin<double>
	{
		public DiffDoubleInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, double.MinValue, double.MaxValue, 0.01)
		{
		}
		
		unsafe protected override void CopyToBuffer(double[] buffer, double* source, int startingIndex, int length)
		{
			if (startingIndex == 0)
				Marshal.Copy(new IntPtr(source), buffer, 0, length);
			else
			{
				fixed (double* destination = buffer)
				{
					double* dst = destination + startingIndex;
					double* src = source + startingIndex;
					
					for (int i = 0; i < length; i++)
						*(dst++) = *(src++);
				}
			}
		}
	}
}
