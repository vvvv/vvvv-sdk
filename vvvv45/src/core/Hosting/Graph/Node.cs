using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.Hosting.Graph
{
    public class Node : IViewableList<INode2>, INode2, IDisposable
    {
        #region nested class InternalNodeListener
        class InternalNodeListener : INodeListener, IDisposable
        {
            private readonly Node FObservedNode;
            
            public InternalNodeListener(Node nodeToObserve)
            {
                FObservedNode = nodeToObserve;
                FObservedNode.InternalCOMInterf.AddListener(this);
            }
            
            public void AddedCB(INode internalChildNode)
            {
                var childNode = Node.Create(internalChildNode, FObservedNode.FNodeInfoFactory);
                FObservedNode.OnAdded(childNode);
            }
            
            public void RemovedCB(INode internalChildNode)
            {
                var childNode = Node.Create(internalChildNode, FObservedNode.FNodeInfoFactory);
                FObservedNode.OnRemoved(childNode);
                childNode.Dispose();
            }
            
            public void PinAddedCB(IPin internalPin)
            {
                if (FObservedNode.FPins != null)
                {
                    var pin = Pin.Create(FObservedNode, internalPin, FObservedNode.FNodeInfoFactory);
                    FObservedNode.FPins.Add(pin);
                }
            }
            
            public void PinRemovedCB(IPin internalPin)
            {
                if (FObservedNode.FPins != null)
                {
                    var pin = internalPin.Tag as Pin;
                    FObservedNode.FPins.Remove(pin);
                    pin.Dispose();
                }
            }
            
            public void LabelChangedCB()
            {
                // TODO: Handle this case properly. See for e.g. Finder/PatchNode or even implement it in delphi code.
                //FObservedNode.Name = FObservedNode.FInternalNode.GetPin("Descriptive Name").GetValue(0);
            }

            public void StatusChangedCB()
            {
                FObservedNode.OnStatusChanged();
            }

            public void InnerStatusChangedCB()
            {
                FObservedNode.OnInnerStatusChanged();
            }
            
            public void BoundsChangedCB(BoundsType boundsType)
            {
                FObservedNode.OnBoundsChanged(boundsType);
            }

            public void Dispose()
            {
                FObservedNode.FInternalCOMInterf.RemoveListener(this);
            }
        }
        #endregion
        
        #region factory methods
        static internal Node Create(INode internalCOMInterf, ProxyNodeInfoFactory nodeInfoFactory)
        {
            var node = internalCOMInterf.Tag as Node;
            if (node == null)
                node = new Node(internalCOMInterf, nodeInfoFactory);
            return node;
        }
        #endregion
        
        #region const defines
        const string LABEL_PIN = "Descriptive Name";
        #endregion
        
        private readonly INode FInternalCOMInterf;
        private readonly ProxyNodeInfoFactory FNodeInfoFactory;
        private InternalNodeListener FInternalNodeListener;
        private int FObserverCount;
        
        private Node(INode internalCOMInterf, ProxyNodeInfoFactory nodeInfoFactory)
        {
            FInternalCOMInterf = internalCOMInterf;
            FNodeInfoFactory = nodeInfoFactory;
            FInternalCOMInterf.Tag = this;
        }
        
        public void Dispose()
        {
            if (FInternalNodeListener != null)
            {
                FInternalNodeListener.Dispose();
                FInternalNodeListener = null;
            }
            
            if (FPins != null)
            {
                foreach (Pin pin in FPins)
                    pin.Dispose();
                FPins.Clear();
            }
            
            if (FLabelPin != null)
                FLabelPin.Changed -= HandleLabelPinChanged;

            if (FNodes != null)
            {
                foreach (Node childNode in FNodes)
                    childNode.Dispose();
                FNodes.Clear();
            }
            
            FNodeInfoFactory.NodeInfoUpdated -= HandleNodeInfoUpdated;
        }

        private void IncObserverCount()
        {
            FObserverCount++;
            if (FInternalNodeListener == null)
                FInternalNodeListener = new InternalNodeListener(this);
        }

        private void DecObserverCount()
        {
            FObserverCount--;
            if (FObserverCount == 0 && FInternalNodeListener != null)
            {
                FInternalNodeListener.Dispose();
                FInternalNodeListener = null;
            }
        }

        void HandleLabelPinChanged(object sender, EventArgs e)
        {
            Name = ComputeName();
        }
        
        public override string ToString()
        {
            return Name;
        }
        
        public INode InternalCOMInterf
        {
            get
            {
                return FInternalCOMInterf;
            }
        }
        
        public INodeInfo NodeInfo
        {
            get
            {
                if (FNodeInfo == null)
                    FNodeInfo = FNodeInfoFactory.ToProxy(FInternalCOMInterf.GetNodeInfo());
                return FNodeInfo;
            }
        }
        INodeInfo FNodeInfo;
        
        public IPin2 LabelPin
        {
            get
            {
                if (FLabelPin == null)
                {
                    FLabelPin = this.FindPin(LABEL_PIN);
                    if (FLabelPin != null)
                        FLabelPin.Changed += HandleLabelPinChanged;
                }
                return FLabelPin;
            }
        }
        IPin2 FLabelPin;
        
        #region INamed
        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
        
        public string Name
        {
            get
            {
                if (FName == null)
                {
                    FName = ComputeName();
                    // Watch node infos
                    FNodeInfoFactory.NodeInfoUpdated += HandleNodeInfoUpdated;
                }
                
                return FName;
            }
            private set
            {
                if (value != FName)
                {
                    OnRenamed(value);
                    FName = value;
                }
            }
        }
        string FName;

        void HandleNodeInfoUpdated(object sender, INodeInfo nodeInfo)
        {
            if (nodeInfo == NodeInfo)
            {
                Name = ComputeName();
            }
        }
        
        private string ComputeName()
        {
            string label = LabelPin[0];
            string suffix = string.IsNullOrEmpty(label) ? string.Empty : " -- " + label;
            
            if (string.IsNullOrEmpty(NodeInfo.Name))
            {
                //subpatches
                string file = System.IO.Path.GetFileNameWithoutExtension(NodeInfo.Filename);
                
                if (string.IsNullOrEmpty(file))
                {
                    //unsaved patch
                    return NodeInfo.Filename + suffix;
                }
                else
                {
                    //patch with valid filename
                    return file + suffix;
                }
            }
            else
            {
                return NodeInfo.Username + suffix;
            }
        }
        #endregion
        
        public int ID
        {
            get
            {
                return FInternalCOMInterf.GetID();
            }
        }
        
        public string GetNodePath(bool UseDescriptiveNames)
        {
            return FInternalCOMInterf.GetNodePath(UseDescriptiveNames);
        }

        public Rectangle GetBounds(BoundsType boundsType)
        {
            return FInternalCOMInterf.GetBounds(boundsType);
        }

        private List<INode2> Nodes
        {
            get
            {
                if (FNodes == null)
                {
                    FNodes = new List<INode2>();
                    var children = FInternalCOMInterf.GetChildren();
                    if (children != null)
                    {
                        foreach (var internalChildNode in children)
                        {
                            var childNode = Node.Create(internalChildNode, FNodeInfoFactory);
                            FNodes.Add(childNode);
                        }
                    }
                    if (HasPatch)
                        IncObserverCount();
                }
                return FNodes;
            }
        }
        List<INode2> FNodes;
        
        public IViewableCollection<IPin2> Pins
        {
            get
            {
                if (FPins == null)
                {
                    FPins = new ViewableCollection<IPin2>();
                    foreach (var internalPin in FInternalCOMInterf.GetPins())
                    {
                        var pin = Pin.Create(this, internalPin, FNodeInfoFactory);
                        FPins.Add(pin);
                    }
                    IncObserverCount();
                }
                return FPins;
            }
        }
        ViewableCollection<IPin2> FPins;
        
        public IWindow2 Window
        {
            get
            {
                var internalWindow = FInternalCOMInterf.Window;
                if (internalWindow != null)
                    return VVVV.Hosting.Graph.Window.Create(internalWindow);
                else
                    return null;
            }
        }
        
        public string LastRuntimeError
        {
            get
            {
                return FInternalCOMInterf.LastRuntimeError;
            }
            set
            {
                FInternalCOMInterf.LastRuntimeError = value;
            }
        }
        
        public INode2 Parent
        {
            get
            {
                var parentNode = FInternalCOMInterf.ParentNode;
                if (parentNode != null)
                    return Node.Create(parentNode, FNodeInfoFactory);
                return null;
            }
        }
        
        public bool HasPatch
        {
            get
            {
                return FInternalCOMInterf.HasPatch;
            }
        }
        
        public bool HasCode
        {
            get
            {
                return FInternalCOMInterf.HasCode;
            }
        }
        
        public bool HasGUI
        {
            get
            {
                return FInternalCOMInterf.HasGUI;
            }
        }
        
        public StatusCode Status
        {
            get
            {
                return FInternalCOMInterf.Status;
            }
            set
            {
                FInternalCOMInterf.Status = value;
            }
        }
        
        public StatusCode InnerStatus
        {
            get
            {
                return FInternalCOMInterf.InnerStatus;
            }
        }
        
        public event EventHandler StatusChanged
        {
            add
            {
                FStatusChanged += value;
                IncObserverCount();
            }
            remove
            {
                FStatusChanged -= value;
                DecObserverCount();
            }
        }
        event EventHandler FStatusChanged;
        
        protected virtual void OnStatusChanged()
        {
            if (FStatusChanged != null)
                FStatusChanged(this, EventArgs.Empty);
        }
        
        public event EventHandler InnerStatusChanged
        {
            add
            {
                FInnerStatusChanged += value;
                IncObserverCount();
            }
            remove
            {
                FInnerStatusChanged -= value;
                DecObserverCount();
            }
        }
        event EventHandler FInnerStatusChanged;
        
        protected virtual void OnInnerStatusChanged()
        {
            if (FInnerStatusChanged != null)
                FInnerStatusChanged(this, EventArgs.Empty);
        }
        
        public event EventHandler<BoundsChangedEventArgs> BoundsChanged
        {
            add
            {
                FBoundsChanged += value;
                IncObserverCount();
            }
            remove
            {
                FBoundsChanged -= value;
                DecObserverCount();
            }
        }
        event EventHandler<BoundsChangedEventArgs> FBoundsChanged;
        
        protected virtual void OnBoundsChanged(BoundsType boundsType)
        {
            if (FBoundsChanged != null)
                FBoundsChanged(this, new BoundsChangedEventArgs(boundsType));
        }

        #region IViewableList<INode2>

        public INode2 this[int index]
        {
            get 
            {
                return Nodes[index];
            }
        }

        public bool Contains(INode2 item)
        {
            return Nodes.Contains(item);
        }

        public event OrderChangedHandler<INode2> OrderChanged
        {
            add
            {
                FOrderChanged += value;
                IncObserverCount();
            }
            remove
            {
                FOrderChanged -= value;
                DecObserverCount();
            }
        }
        event OrderChangedHandler<INode2> FOrderChanged;

        protected void OnOrderChanged()
        {
            if (FOrderChanged != null)
                FOrderChanged(this);
            if (FOrderChanged_ != null)
                FOrderChanged_(this);
        }

        public event CollectionDelegate<INode2> Added
        {
            add
            {
                FAdded += value;
                IncObserverCount();
            }
            remove
            {
                FAdded -= value;
                DecObserverCount();
            }
        }
        event CollectionDelegate<INode2> FAdded;

        protected void OnAdded(INode2 node)
        {
            if (FNodes != null)
                FNodes.Add(node);
            if (FAdded != null)
                FAdded(this, node);
            if (FAdded_ != null)
                FAdded_(this, node);
        }

        public event CollectionDelegate<INode2> Removed
        {
            add
            {
                FRemoved += value;
                IncObserverCount();
            }
            remove
            {
                FRemoved -= value;
                DecObserverCount();
            }
        }
        event CollectionDelegate<INode2> FRemoved;

        protected void OnRemoved(INode2 node)
        {
            if (FNodes != null)
                FNodes.Remove(node);
            if (FRemoved != null)
                FRemoved(this, node);
            if (FRemoved_ != null)
                FRemoved_(this, node);
        }

        public event CollectionUpdateDelegate<INode2> Cleared
        {
            add
            {
                FCleared += value;
                IncObserverCount();
            }
            remove
            {
                FCleared -= value;
                DecObserverCount();
            }
        }
        event CollectionUpdateDelegate<INode2> FCleared;

        protected void OnCleared()
        {
            // Clear the cache
            FNodes = null;
            if (FCleared != null)
                FCleared(this);
            if (FCleared_ != null)
                FCleared_(this);
        }

        public IEnumerator<INode2> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return Nodes.Count; }
        }

        public bool Contains(object item)
        {
            var node = item as INode2;
            if (node != null)
                return Contains(node);
            return false;
        }

        event CollectionDelegate IViewableCollection.Added
        {
            add 
            { 
                FAdded_ += value;
                IncObserverCount();
            }
            remove 
            { 
                FAdded_ -= value;
                DecObserverCount();
            }
        }
        CollectionDelegate FAdded_;

        event CollectionDelegate IViewableCollection.Removed
        {
            add 
            {
                FRemoved_ += value;
                IncObserverCount();
            }
            remove 
            {
                FRemoved_ -= value;
                DecObserverCount();
            }
        }
        event CollectionDelegate FRemoved_;

        event CollectionUpdateDelegate IViewableCollection.Cleared
        {
            add 
            {
                FCleared_ += value;
                IncObserverCount();
            }
            remove 
            {
                FCleared_ -= value;
                DecObserverCount();
            }
        }
        event CollectionUpdateDelegate FCleared_;

        public event CollectionUpdateDelegate UpdateBegun;

        protected void OnUpdateBegun()
        {
            if (UpdateBegun != null)
                UpdateBegun(this);
        }

        public event CollectionUpdateDelegate Updated;

        protected void OnUpdated()
        {
            if (Updated != null)
                Updated(this);
        }

        object IViewableList.this[int index]
        {
            get { return this[index]; }
        }

        event OrderChangedHandler IViewableList.OrderChanged
        {
            add 
            {
                FOrderChanged_ += value;
                IncObserverCount();
            }
            remove 
            {
                FOrderChanged_ -= value;
                DecObserverCount();
            }
        }
        event OrderChangedHandler FOrderChanged_;

        #endregion
    }
}
