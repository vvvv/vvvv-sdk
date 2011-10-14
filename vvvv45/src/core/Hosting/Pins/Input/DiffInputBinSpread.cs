using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class DiffInputBinSpread<T> : InputBinSpread<T>, IDiffSpread<ISpread<T>>, IDisposable
	{
		protected DiffInputPin<T> FDiffSpreadPin;

		public DiffInputBinSpread(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			FDiffSpreadPin = (DiffInputPin<T>) FSpreadPin;
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
		    if (FChanged != null)
		        FChanged(this);
		}

		public event SpreadChangedEventHander<ISpread<T>> Changed;
		
		protected SpreadChangedEventHander FChanged;
        event SpreadChangedEventHander IDiffSpread.Changed
        {
            add
            {
                FChanged += value;
            }
            remove
            {
                FChanged -= value;
            }
        }
		
		public DiffInputPin<T> SpreadPin 
		{ 
			get 
			{ 
				return this.FDiffSpreadPin; 
			} 
		}
		
		public bool IsChanged 
		{
			get 
			{
				return FDiffSpreadPin.IsChanged || FBinSizePin.IsChanged;
			}
		}

		protected override Pin<T> CreateDataPin(IPluginHost host, InputAttribute attribute)
		{
			return PinFactory.CreateDiffPin<T>(host, attribute);
		}
		
		protected override bool NeedToBuildSpread()
		{
			return IsChanged;
		}			
	}
}
