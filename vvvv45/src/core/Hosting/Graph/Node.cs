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
    public class Node : ViewableList<INode2>, INode2
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
                
                // HACK: We need to check if node not already in collection from Node.ctor -> GetChildren
                if (!FObservedNode.Contains(childNode))
                    FObservedNode.Add(childNode);
            }
            
            public void RemovedCB(INode internalChildNode)
            {
                // TODO: Sometimes RemovedCB is called with out an AddedCB before.
                var childNode = internalChildNode.Tag as Node;
                if (childNode != null)
                {
                    FObservedNode.Remove(childNode);
                    childNode.Dispose();
                }
            }
            
            public void PinAddedCB(IPin internalPin)
            {
                var pins = FObservedNode.FPins.Value;
                var pin = Pin.Create(FObservedNode, internalPin, FObservedNode.FNodeInfoFactory);
                pins.Add(pin);
            }
            
            public void PinRemovedCB(IPin internalPin)
            {
                var pins = FObservedNode.FPins.Value;
                var pin = internalPin.Tag as Pin;
                pins.Remove(pin);
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
            {
                node = new Node(internalCOMInterf, nodeInfoFactory);
            }
            return node;
        }
        #endregion
        
        #region const defines
        const string LABEL_PIN = "Descriptive Name";
        #endregion
        
        private readonly INode FInternalCOMInterf;
        private readonly INodeInfo FNodeInfo;
        private readonly InternalNodeListener FInternalNodeListener;
        private readonly ProxyNodeInfoFactory FNodeInfoFactory;
        private readonly Lazy<ViewableCollection<IPin2>> FPins;
        private readonly Lazy<IPin2> FLabelPin;
        
        private Node(INode internalCOMInterf, ProxyNodeInfoFactory nodeInfoFactory)
        {
            FInternalCOMInterf = internalCOMInterf;
            FNodeInfoFactory = nodeInfoFactory;
            
            FNodeInfo = nodeInfoFactory.ToProxy(internalCOMInterf.GetNodeInfo());
            
            FPins = new Lazy<ViewableCollection<IPin2>>(InitPins);
            FLabelPin = new Lazy<IPin2>(InitLabelPin);

            var children = internalCOMInterf.GetChildren();
            if (children != null)
            {
                foreach (var internalChildNode in children)
                {
                    var childNode = Node.Create(internalChildNode, nodeInfoFactory);
                    Add(childNode);
                }
            }
            
            FInternalNodeListener = new InternalNodeListener(this);
            FInternalCOMInterf.Tag = this;
        }
        
        public override void Dispose()
        {
            FInternalNodeListener.Dispose();
            
            if (FPins.IsValueCreated)
            {
                foreach (Pin pin in Pins)
                    pin.Dispose();
                
                FPins.Value.Dispose();
            }
            
            if (FLabelPin.IsValueCreated)
            {
                FLabelPin.Value.Changed -= HandleLabelPinChanged;
            }
            
            var childNodes = this.ToList();
            foreach (Node childNode in childNodes)
            {
                this.Remove(childNode);
                childNode.Dispose();
            }
            childNodes.Clear();
            
            FNodeInfoFactory.NodeInfoUpdated -= HandleNodeInfoUpdated;
            
            base.Dispose();
        }
        
        private ViewableCollection<IPin2> InitPins()
        {
            var pins = new ViewableCollection<IPin2>();
            foreach (var internalPin in FInternalCOMInterf.GetPins())
            {
                var pin = Pin.Create(this, internalPin, FNodeInfoFactory);
                pins.Add(pin);
            }
            return pins;
        }
        
        private IPin2 InitLabelPin()
        {
            var labelPin = this.FindPin(LABEL_PIN);
            labelPin.Changed += HandleLabelPinChanged;
            return labelPin;
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
                return FNodeInfo;
            }
        }
        
        public IPin2 LabelPin
        {
            get
            {
                return FLabelPin.Value;
            }
        }
        
        #region INamed
        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
        
        private string FName;
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

        void HandleNodeInfoUpdated(object sender, INodeInfo nodeInfo)
        {
            if (nodeInfo == FNodeInfo)
            {
                Name = ComputeName();
            }
        }
        
        private string ComputeName()
        {
            string label = FLabelPin.Value[0];
            string suffix = string.IsNullOrEmpty(label) ? string.Empty : " -- " + label;
            
            if (string.IsNullOrEmpty(FNodeInfo.Name))
            {
                //subpatches
                string file = System.IO.Path.GetFileNameWithoutExtension(FNodeInfo.Filename);
                
                if (string.IsNullOrEmpty(file))
                {
                    //unsaved patch
                    return FNodeInfo.Filename + suffix;
                }
                else
                {
                    //patch with valid filename
                    return file + suffix;
                }
            }
            else
            {
                return FNodeInfo.Username + suffix;
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
        
        public IViewableCollection<IPin2> Pins
        {
            get
            {
                return FPins.Value;
            }
        }
        
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
                if (FInternalCOMInterf.ParentNode == null)
                    return null;
                else
                    return Node.Create(FInternalCOMInterf.ParentNode, FNodeInfoFactory);
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
        
        public event EventHandler StatusChanged;
        
        protected virtual void OnStatusChanged()
        {
            if (StatusChanged != null)
                StatusChanged(this, EventArgs.Empty);
        }
        
        public event EventHandler InnerStatusChanged;
        
        protected virtual void OnInnerStatusChanged()
        {
            if (InnerStatusChanged != null)
                InnerStatusChanged(this, EventArgs.Empty);
        }
        
        public event EventHandler<BoundsChangedEventArgs> BoundsChanged;
        
        protected virtual void OnBoundsChanged(BoundsType boundsType)
        {
            if (BoundsChanged != null)
                BoundsChanged(this, new BoundsChangedEventArgs(boundsType));
        }
    }
}
