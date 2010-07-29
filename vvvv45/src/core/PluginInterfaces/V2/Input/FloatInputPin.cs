using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	public class FloatInputPin : ValueInputPin<float>
	{
		public FloatInputPin(IPluginHost host, InputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override float this[int index] 
		{
			get 
			{
				double value;
				FValueIn.GetValue(index, out value);
				return (float) value;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
	}
}
