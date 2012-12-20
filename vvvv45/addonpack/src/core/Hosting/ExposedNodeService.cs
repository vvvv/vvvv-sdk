using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VVVV.Hosting.Graph;
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
            private readonly ProxyNodeInfoFactory FProxyNodeInfoFactory;
            
            public ExposedNodeListener(ExposedNodeService exposedNodeService, ProxyNodeInfoFactory proxyNodeInfoFactory)
            {
                FExposedNodeService = exposedNodeService;
                FProxyNodeInfoFactory = proxyNodeInfoFactory;
            }
            
            public void NodeAddedCB(INode node)
            {
            	FExposedNodeService.OnNodeAdded(Node.Create(node, FProxyNodeInfoFactory));
            }
            
            public void NodeRemovedCB(INode node)
            {
            	FExposedNodeService.OnNodeRemoved(Node.Create(node, FProxyNodeInfoFactory));
            }
        }
        
        private readonly IInternalExposedNodeService FExposedNodeService;
        private readonly ExposedNodeListener FExposedNodeListener;
        private readonly ProxyNodeInfoFactory FProxyNodeInfoFactory;
        
        public ExposedNodeService(IInternalExposedNodeService exposedNodeService, ProxyNodeInfoFactory proxyNodeInfoFactory)
        {
            FExposedNodeService = exposedNodeService;
            FProxyNodeInfoFactory = proxyNodeInfoFactory;
            FExposedNodeListener = new ExposedNodeListener(this, proxyNodeInfoFactory);
            FExposedNodeService.Subscribe(FExposedNodeListener);
        }
        
        public void Dispose()
        {
            FExposedNodeService.Unsubscribe(FExposedNodeListener);
        }
        
        public event NodeEventHandler NodeAdded;
        protected virtual void OnNodeAdded(INode2 node)
        {
            if (NodeAdded != null) 
            {
                NodeAdded(node);
            }
        }
        
        public event NodeEventHandler NodeRemoved;
        protected virtual void OnNodeRemoved(INode2 node)
        {
            if (NodeRemoved != null) 
            {
                NodeRemoved(node);
            }
        }
        
        public IEnumerable<INode2> Nodes
        {
            get 
            {
                foreach (var node in FExposedNodeService.Nodes)
                {
                    yield return Node.Create(node, FProxyNodeInfoFactory);
                }
            }
        }
    }
}
