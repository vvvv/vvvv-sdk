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
        }
        
        #endregion
        
        private readonly IPin FInternalCOMInterf;
        private readonly string FName;
        private readonly PinListener FPinListener;
        
        internal Pin(IPin internalCOMInterf)
        {
            FInternalCOMInterf = internalCOMInterf;
            FName = FInternalCOMInterf.GetName();
            FPinListener = new PinListener(this);
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
        
        public event EventHandler Changed;
        
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null) {
                Changed(this, e);
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
        
        public bool IsConnected 
        {
            get 
            {
                return FInternalCOMInterf.IsConnected();
            }
        }
        
        public string Name
        {
            get
            {
                return FName;
            }
        }
    }
}
