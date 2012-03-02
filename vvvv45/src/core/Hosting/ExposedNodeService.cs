using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.Hosting
{
    [ComVisible(false)]
    class ExposedNodeService: IExposedNodeService, IDisposable
    {
        class ExposedNodeListener: IInternalExposedNodeListener
        {
            private readonly ExposedNodeService FExposedNodeService;
            
            public ExposedNodeListener(ExposedNodeService exposedNodeService)
            {
                FExposedNodeService = exposedNodeService;
            }
            
            public void NodeAddedCB(INode node)
            {
                FExposedNodeService.OnNodeAdded(node);
            }
            
            public void NodeRemovedCB(INode node)
            {
                FExposedNodeService.OnNodeRemoved(node);
            }
        }
        
        private readonly IInternalExposedNodeService FExposedNodeService;
        private readonly ExposedNodeListener FExposedNodeListener;
        
        public ExposedNodeService(IInternalExposedNodeService exposedNodeService)
        {
            FExposedNodeService = exposedNodeService;
            FExposedNodeListener = new ExposedNodeListener(this);
            FExposedNodeService.Subscribe(FExposedNodeListener);
        }
        
        public void Dispose()
        {
            FExposedNodeService.Unsubscribe(FExposedNodeListener);
        }
        
        public event NodeEventHandler NodeAdded;
        protected virtual void OnNodeAdded(NodeEventHandler e)
        {
            if (NodeAdded != null) 
            {
                NodeAdded(e);
            }
        }
        
        public event NodeEventHandler NodeRemoved;
        protected virtual void OnNodeRemoved(NodeEventHandler e)
        {
            if (NodeRemoved != null) 
            {
                NodeRemoved(e);
            }
        }
        
        public IEnumerable<INode2> Nodes
        {
            get 
            {
                foreach (var node in FExposedNodeService.Nodes)
                {
                    yield return node;
                }
            }
        }
    }
}
