using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public class BoolInputPin : ValueInputPin<bool>
	{
		public BoolInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override bool this[int index] 
		{
			get 
			{
				double value;
				FValueIn.GetValue(index, out value);
				return value >= 0.5;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
	}
}
