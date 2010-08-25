using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class DiffInputBinSpread<T> : InputBinSpread<T>, IDiffSpread<ISpread<T>>
	{
		protected DiffInputWrapperPin<T> FDiffSpreadPin;

		public DiffInputBinSpread(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			FDiffSpreadPin = (DiffInputWrapperPin<T>)FSpreadPin;
			FDiffSpreadPin.Changed += new SpreadChangedEventHander<T>(FSpreadPin_Changed);
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
				return FDiffSpreadPin.IsChanged;
			}
		}

		protected virtual void CreateWrapperPin(IPluginHost host, InputAttribute attribute)
		{
			FSpreadPin = new DiffInputWrapperPin<T>(host, attribute);
		}
		
		protected override bool NeedToBuildSpread()
		{
			return (FBinSize.IsChanged || FDiffSpreadPin.IsChanged);
		}			
	}
}
