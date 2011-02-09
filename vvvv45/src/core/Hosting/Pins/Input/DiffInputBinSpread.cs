using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
	public class DiffInputBinSpread<T> : InputBinSpread<T>, IDiffSpread<ISpread<T>>, IDisposable
	{
		protected DiffPin<T> FDiffSpreadPin;

		public DiffInputBinSpread(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			FDiffSpreadPin = (DiffPin<T>) FSpreadPin;
			FDiffSpreadPin.Changed += FSpreadPin_Changed;
			FBinSizePin.Changed += FBinSizePin_Changed;
		}
		
        public override void Dispose()
        {
            FDiffSpreadPin.Changed -= FSpreadPin_Changed;
            FBinSizePin.Changed -= FBinSizePin_Changed;
            base.Dispose();
        }

		void FSpreadPin_Changed(IDiffSpread<T> spread)
		{
			OnChanged();
		}
		
		void FBinSizePin_Changed(IDiffSpread<int> spread)
		{
		    OnChanged();
		}
		
		protected virtual void OnChanged()
		{
		    if (Changed != null)
		        Changed(this);
		}

		public event SpreadChangedEventHander<ISpread<T>> Changed;
		
		public bool IsChanged 
		{
			get 
			{
				return FDiffSpreadPin.IsChanged || FBinSizePin.IsChanged;
			}
		}

		protected override void CreateDataPin(IPluginHost host, InputAttribute attribute)
		{
			FSpreadPin = PinFactory.CreateDiffPin<T>(host, attribute);
		}
		
		protected override bool NeedToBuildSpread()
		{
			return IsChanged;
		}			
	}
}
