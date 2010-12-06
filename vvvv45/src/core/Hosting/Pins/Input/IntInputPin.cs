using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class IntInputPin : ValueInputPin<int>
	{
		public IntInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, int.MinValue, int.MaxValue, 1)
		{
		}
		
		unsafe protected override void CopyToBuffer(int[] buffer, double* source, int startingIndex, int length)
		{
			fixed (int* destination = buffer)
			{
				int* dst = destination + startingIndex;
				double* src = source + startingIndex;
				
				for (int i = 0; i < length; i++)
					*(dst++) = (int) Math.Round(*(src++));
			}
		}
	}
	
	public class DiffIntInputPin : DiffValueInputPin<int>
	{
		public DiffIntInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, int.MinValue, int.MaxValue, 1)
		{
		}
		
		unsafe protected override void CopyToBuffer(int[] buffer, double* source, int startingIndex, int length)
		{
			fixed (int* destination = buffer)
			{
				int* dst = destination + startingIndex;
				double* src = source + startingIndex;
				
				for (int i = 0; i < length; i++)
					*(dst++) = (int) Math.Round(*(src++));
			}
		}
	}
}
