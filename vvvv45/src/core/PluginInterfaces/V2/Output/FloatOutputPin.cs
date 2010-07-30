using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	public class FloatOutputPin : ValueOutputPin<float>
	{
		public FloatOutputPin(IPluginHost host, OutputAttribute attribute)
			:base(host, attribute)
		{
		}
		
		public override float this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				FValueOut.SetValue(index, (double) value);
			}
		}
	}
}
