using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class UIntInputPin : ValueInputPin<uint>
	{
		public UIntInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, uint.MinValue, uint.MaxValue, 1)
		{
		}
		
		unsafe protected override void CopyToBuffer(uint[] buffer, double* source, int startingIndex, int length)
		{
			fixed (uint* destination = buffer)
			{
				uint* dst = destination + startingIndex;
				double* src = source + startingIndex;
				
				for (int i = 0; i < length; i++)
					*(dst++) = (uint) Math.Round(*(src++));
			}
		}
	}
	
	[ComVisible(false)]
	public class DiffUIntInputPin : DiffValueInputPin<uint>
	{
		public DiffUIntInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, uint.MinValue, uint.MaxValue, 1)
		{
		}
		
		unsafe protected override void CopyToBuffer(uint[] buffer, double* source, int startingIndex, int length)
		{
			fixed (uint* destination = buffer)
			{
				uint* dst = destination + startingIndex;
				double* src = source + startingIndex;
				
				for (int i = 0; i < length; i++)
					*(dst++) = (uint) Math.Round(*(src++));
			}
		}
	}
}
