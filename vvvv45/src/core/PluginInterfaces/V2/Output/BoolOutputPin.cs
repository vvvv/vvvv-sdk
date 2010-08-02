using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class BoolOutputPin : ValueOutputPin<bool>
	{
		public BoolOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override bool this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set
			{
				FValueOut.SetValue(index, value ? 1 : 0);
			}
		}
	}
}
