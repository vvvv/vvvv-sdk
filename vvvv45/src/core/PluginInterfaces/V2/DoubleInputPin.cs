using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
	class DoubleInputPin : InputPin<double>
	{
		protected IValueIn FValueIn;
		
		[ImportingConstructor]
		public DoubleInputPin(IPluginHost host, string name)
		{
			host.CreateValueInput(name, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueIn);
			FValueIn.SetSubType(double.MinValue, double.MaxValue, 1.0, 0.0, false, false, false);
		}
		
		public override double this[int index] 
		{
			get 
			{
				double value;
				FValueIn.GetValue(index, out value);
				return value;
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
		
		public override int SliceCount 
		{
			get 
			{
				return FValueIn.SliceCount;
			}
			set {
				throw new NotImplementedException();
			}
		}
	}
}
