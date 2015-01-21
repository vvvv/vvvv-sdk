using System;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils;

namespace VVVV.Hosting.Graph
{
    public class Pin: Disposable, IPin2
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
            
            public void SubtypeChangedCB()
            {
                FPin.OnSubtypeChanged();
            }
            
            public void ConnectedCB(IPin otherPin)
            {
                FPin.OnConnected(new PinConnectionEventArgs(otherPin));
            }
            
            public void DisconnectedCB(IPin otherPin)
            {
                FPin.OnDisconnected(new PinConnectionEventArgs(otherPin));
            }
        }
        
        #endregion
        
        #region factory methods
        static internal Pin Create(INode2 node, IPin internalCOMInterf, ProxyNodeInfoFactory nodeInfoFactory)
        {
            var pin = internalCOMInterf.Tag as Pin;
            if (pin == null)
            {
                pin = new Pin(node, internalCOMInterf, nodeInfoFactory);
            }
            return pin;
        }
        #endregion
        
        private INode2 FParentNode;
        private readonly IPin FInternalCOMInterf;
        private readonly string FName;
        private readonly ProxyNodeInfoFactory FNodeInfoFactory;
        private PinListener FPinListener;
        private int FObserverCount;
        
        private Pin(INode2 node, IPin internalCOMInterf, ProxyNodeInfoFactory nodeInfoFactory)
        {
            FParentNode = node;
            FInternalCOMInterf = internalCOMInterf;
            FNodeInfoFactory = nodeInfoFactory;
            FName = FInternalCOMInterf.Name;
            FInternalCOMInterf.Tag = this;
        }
        
        protected override void DisposeManaged()
        {
            if (FPinListener != null)
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

        /// <summary>
        /// Reference to the internal COM interface. Use with caution.
        /// </summary>
        public IPin InternalCOMInterf
        {
            get { return this.FInternalCOMInterf; }
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
        
        public string NameByParent(INode2 parentNode)
        {
            return FInternalCOMInterf.GetNameByParent(parentNode.InternalCOMInterf);
        }
        
        public INode2 ParentNode
        {
            get
            {
                if (FParentNode == null)
                {
                    FParentNode = Node.Create(FInternalCOMInterf.ParentNode, FNodeInfoFactory);
                }
                return FParentNode;
            }
        }
        
        public INode2 ParentNodeByPatch(INode2 patch)
        {
            var node = FInternalCOMInterf.GetParentNodeByPatch(patch.InternalCOMInterf);
            if (node != null)
                return Node.Create(node, FNodeInfoFactory);
            else
                return null;
        }
        
        public string this[int sliceIndex]
        {
            get
            {
                try
                {
                    return FInternalCOMInterf.GetSlice(sliceIndex);
                }
                catch
                {
                    return string.Empty;
                }
            }
            
            set
            {
                FInternalCOMInterf.SetSlice(sliceIndex, value);
            }
        }
        
        public string Spread
        {
            get
            {
                try
                {
                    return FInternalCOMInterf.GetSpread();
                }
                catch
                {
                    return string.Empty;
                }
            }
            
            set
            {
                FInternalCOMInterf.SetSpread(value);
            }
        }

        public int SliceCount
        {
            get
            {
                return FInternalCOMInterf.SliceCount;
            }
        }
        
        public PinDirection Direction
        {
            get
            {
                return FInternalCOMInterf.Direction;
            }
        }
        
        public IViewableCollection<IPin2> ConnectedPins
        {
            get
            {
                var pins = new ViewableCollection<IPin2>();
                foreach (var internalPin in FInternalCOMInterf.GetConnectedPins())
                {
                    var node = Node.Create(internalPin.ParentNode, FNodeInfoFactory);
                    pins.Add(new Pin(node, internalPin, FNodeInfoFactory));
                }
                return pins;
            }
        }
        
        public string Type
        {
            get
            {
                return FInternalCOMInterf.Type;
            }
        }
        
        public Type CLRType
        {
            get
            {
                return FInternalCOMInterf.CLRType;
            }
        }
        
        public string SubType
        {
            get
            {
                return FInternalCOMInterf.SubType;
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
        
        private event EventHandler FSubtypeChanged;
        public event EventHandler SubtypeChanged
        {
            add
            {
                IncObserverCount();
                FSubtypeChanged += value;
            }
            remove
            {
                FSubtypeChanged -= value;
                DecObserverCount();
            }
        }
        
        protected virtual void OnSubtypeChanged()
        {
            if (FSubtypeChanged != null)
                FSubtypeChanged(this, EventArgs.Empty);
        }
        
        private event PinConnectionEventHandler FConnected;
        public event PinConnectionEventHandler Connected
        {
            add
            {
                IncObserverCount();
                FConnected += value;
            }
            remove
            {
                FConnected -= value;
                DecObserverCount();
            }
        }
        
        protected virtual void OnConnected(PinConnectionEventArgs args)
        {
            if (FConnected != null) {
                FConnected(this, args);
            }
        }
        
        private event PinConnectionEventHandler FDisconnected;
        public event PinConnectionEventHandler Disconnected
        {
            add
            {
                IncObserverCount();
                FDisconnected += value;
            }
            remove
            {
                FDisconnected -= value;
                DecObserverCount();
            }
        }
        
        protected virtual void OnDisconnected(PinConnectionEventArgs args)
        {
            if (FDisconnected != null) {
                FDisconnected(this, args);
            }
        }
    }
}
