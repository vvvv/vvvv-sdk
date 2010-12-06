using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class BoolInputPin : ValueInputPin<bool>
	{
		public BoolInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 0.0, 1.0, 1.0)
		{
		}
		
		unsafe protected override void CopyToBuffer(bool[] buffer, double* source, int startingIndex, int length)
		{
			fixed (bool* destination = buffer)
			{
				bool* dst = destination + startingIndex;
				double* src = source + startingIndex;
				
				for (int i = 0; i < length; i++)
					*(dst++) = *(src++) >= 0.5;
			}
		}
	}
	
	public class DiffBoolInputPin : DiffValueInputPin<bool>
	{
		public DiffBoolInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute, 0.0, 1.0, 1.0)
		{
		}
		
		unsafe protected override void CopyToBuffer(bool[] buffer, double* source, int startingIndex, int length)
		{
			fixed (bool* destination = buffer)
			{
				bool* dst = destination + startingIndex;
				double* src = source + startingIndex;
				
				for (int i = 0; i < length; i++)
					*(dst++) = *(src++) >= 0.5;
			}
		}
	}
}
