using System;
using VVVV.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils;

namespace VVVV.Hosting.Graph
{
    internal class Pin : Disposable, IPin2
    {
        #region IPinListener implementation
        
        class PinListener : Disposable, IPinListener
        {
            private readonly Pin FPin;
            
            public PinListener(Pin pin)
            {
                FPin = pin;
                
                FPin.FInternalCOMInterf.AddListener(this);
            }
            protected override void DisposeManaged()
            {
                FPin.FInternalCOMInterf.RemoveListener(this);
                
                base.DisposeManaged();
            }
            
            public void ChangedCB()
            {
                FPin.OnChanged(EventArgs.Empty);
            }
			
			public void StatusChangedCB()
			{
				FPin.OnStatusChanged();
			}
        }
        
        #endregion
        
        private readonly IPin FInternalCOMInterf;
        private readonly string FName;
        private PinListener FPinListener;
        private int FObserverCount;
        
        internal Pin(IPin internalCOMInterf)
        {
            FInternalCOMInterf = internalCOMInterf;
            FName = FInternalCOMInterf.GetName();
        }
        
        protected override void DisposeManaged()
        {
            FPinListener.Dispose();
            base.DisposeManaged();
        }
        
        public override string ToString()
        {
            return FName;
        }
        
        private void IncObserverCount()
        {
        	FObserverCount++;
        	if (FPinListener == null)
        		FPinListener = new PinListener(this);
        }
        
        private void DecObserverCount()
        {
        	FObserverCount--;
        	if (FObserverCount == 0)
        	{
        		FPinListener.Dispose();
        		FPinListener = null;
        	}
        }
        
        private event EventHandler FChanged;
        public event EventHandler Changed
        {
        	add
        	{
        		IncObserverCount();
        		FChanged += value;
        	}
        	remove
        	{
        		FChanged -= value;
        		DecObserverCount();
        	}
        }
        
        protected virtual void OnChanged(EventArgs e)
        {
            if (FChanged != null) {
                FChanged(this, e);
            }
        }
        
        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
        
        public string this[int sliceIndex] 
        {
            get 
            {
                return FInternalCOMInterf.GetValue(sliceIndex);
            }
        }
        
        public string Name
        {
            get
            {
                return FName;
            }
        }
		
		public StatusCode Status
		{
			get
			{
				return FInternalCOMInterf.Status;
			}
		}
		
		private event EventHandler FStatusChanged;
		public event EventHandler StatusChanged
		{
			add
			{
				IncObserverCount();
				FStatusChanged += value;
			}
			remove
			{
				FStatusChanged -= value;
				DecObserverCount();
			}
		}
        
        protected virtual void OnStatusChanged()
        {
            if (FStatusChanged != null)
                FStatusChanged(this, EventArgs.Empty);
        }
    }
}
