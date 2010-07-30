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
				if (value)
					FValueOut.SetValue(index, 1.0);
				else
					FValueOut.SetValue(index, 0.0);
			}
		}
	}
}
