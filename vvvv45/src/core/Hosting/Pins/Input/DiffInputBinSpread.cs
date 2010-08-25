using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class DiffInputBinSpread<T> : InputBinSpread<T>, IDiffSpread<ISpread<T>>
	{
		public DiffInputBinSpread(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			FSpreadPin.Changed += new SpreadChangedEventHander<T>(FSpreadPin_Changed);
		}

		void FSpreadPin_Changed(IDiffSpread<T> spread)
		{
			if (Changed != null) 
				Changed(this);
		}

		public event SpreadChangedEventHander<ISpread<T>> Changed;
		
		public bool IsChanged 
		{
			get 
			{
				return FSpreadPin.IsChanged;
			}
		}
		
	}
}
