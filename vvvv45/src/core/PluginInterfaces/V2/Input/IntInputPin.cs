using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class IntInputPin : ValueInputPin<int>
	{
		public IntInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override int this[int index] 
		{
			get 
			{
				double value;
				FValueIn.GetValue(index, out value);
				return (int) value;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
	}
}
