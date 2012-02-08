using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
    [ComVisible(false)]
    class GraphEventService : IGraphEventService
    {
        class PrepareGraphListener : IInternalGraphEventListener
        {
            private readonly GraphEventService FService;
            
            public PrepareGraphListener(GraphEventService service)
            {
                FService = service;
            }
            
            public void HandleEvent()
            {
                FService.CallOnPrepareGraph(EventArgs.Empty);
            }
        }
        
        class UpdateViewListener : IInternalGraphEventListener
        {
            private readonly GraphEventService FService;
            
            public UpdateViewListener(GraphEventService service)
            {
                FService = service;
            }
            
            public void HandleEvent()
            {
                FService.CallOnUpdateView(EventArgs.Empty);
            }
        }
        
        class RenderListener : IInternalGraphEventListener
        {
            private readonly GraphEventService FService;
            
            public RenderListener(GraphEventService service)
            {
                FService = service;
            }
            
            public void HandleEvent()
            {
                FService.CallOnRender(EventArgs.Empty);
            }
        }
        
        class PresentListener : IInternalGraphEventListener
        {
            private readonly GraphEventService FService;
            
            public PresentListener(GraphEventService service)
            {
                FService = service;
            }
            
            public void HandleEvent()
            {
                FService.CallOnPresent(EventArgs.Empty);
            }
        }
        
        class DebugListener : IInternalGraphEventListener
        {
            private readonly GraphEventService FService;
            
            public DebugListener(GraphEventService service)
            {
                FService = service;
            }
            
            public void HandleEvent()
            {
                FService.CallOnDebug(EventArgs.Empty);
            }
        }
        
        class NetworkSyncListener : IInternalGraphEventListener
        {
            private readonly GraphEventService FService;
            
            public NetworkSyncListener(GraphEventService service)
            {
                FService = service;
            }
            
            public void HandleEvent()
            {
                FService.CallOnNetworkSync(EventArgs.Empty);
            }
        }
        
        class ResetCacheListener : IInternalGraphEventListener
        {
            private readonly GraphEventService FService;
            
            public ResetCacheListener(GraphEventService service)
            {
                FService = service;
            }
            
            public void HandleEvent()
            {
                FService.CallOnResetCache(EventArgs.Empty);
            }
        }
        
        private readonly IInternalGraphEventService FGraphEventService;
        private readonly PrepareGraphListener FPrepareGraphListener;
        private readonly UpdateViewListener FUpdateViewListener;
        private readonly RenderListener FRenderListener;
        private readonly PresentListener FPresentListener;
        private readonly DebugListener FDebugListener;
        private readonly NetworkSyncListener FNetworkSyncListener;
        private readonly ResetCacheListener FResetCacheListener;
        
        public GraphEventService(IInternalGraphEventService graphEventService)
        {
            FGraphEventService = graphEventService;
            FPrepareGraphListener = new PrepareGraphListener(this);
            FUpdateViewListener = new UpdateViewListener(this);
            FRenderListener = new RenderListener(this);
            FPresentListener = new PresentListener(this);
            FDebugListener = new DebugListener(this);
            FNetworkSyncListener = new NetworkSyncListener(this);
            FResetCacheListener = new ResetCacheListener(this);
        }
        
        private int FOnPrepareGraphCount;
        private event EventHandler FOnPrepareGraph;
        public event EventHandler OnPrepareGraph
        {
            add
            {
                if (FOnPrepareGraphCount == 0)
                {
                    FGraphEventService.OnPrepareGraph.Subscribe(FPrepareGraphListener);
                }
                FOnPrepareGraphCount++;
                FOnPrepareGraph += value;
            }
            remove
            {
                FOnPrepareGraph -= value;
                FOnPrepareGraphCount--;
                if (FOnPrepareGraphCount == 0)
                {
                    FGraphEventService.OnPrepareGraph.Unsubscribe(FPrepareGraphListener);
                }
            }
        }
        
        protected virtual void CallOnPrepareGraph(EventArgs e)
        {
            if (FOnPrepareGraph != null) 
            {
                FOnPrepareGraph(this, e);
            }
        }
        
        private int FOnUpdateViewCount;
        private event EventHandler FOnUpdateView;
        public event EventHandler OnUpdateView
        {
            add
            {
                if (FOnUpdateViewCount == 0)
                {
                    FGraphEventService.OnUpdateView.Subscribe(FUpdateViewListener);
                }
                FOnUpdateViewCount++;
                FOnUpdateView += value;
            }
            remove
            {
                FOnUpdateView -= value;
                FOnUpdateViewCount--;
                if (FOnUpdateViewCount == 0)
                {
                    FGraphEventService.OnUpdateView.Unsubscribe(FUpdateViewListener);
                }
            }
        }
        
        protected virtual void CallOnUpdateView(EventArgs e)
        {
            if (FOnUpdateView != null) 
            {
                FOnUpdateView(this, e);
            }
        }
        
        private int FOnRenderCount;
        private event EventHandler FOnRender;
        public event EventHandler OnRender
        {
            add
            {
                if (FOnRenderCount == 0)
                {
                    FGraphEventService.OnRender.Subscribe(FRenderListener);
                }
                FOnRenderCount++;
                FOnRender += value;
            }
            remove
            {
                FOnRender -= value;
                FOnRenderCount--;
                if (FOnRenderCount == 0)
                {
                    FGraphEventService.OnRender.Unsubscribe(FRenderListener);
                }
            }
        }
        
        protected virtual void CallOnRender(EventArgs e)
        {
            if (FOnRender != null) 
            {
                FOnRender(this, e);
            }
        }
        
        private int FOnPresentCount;
        private event EventHandler FOnPresent;
        public event EventHandler OnPresent
        {
            add
            {
                if (FOnPresentCount == 0)
                {
                    FGraphEventService.OnPresent.Subscribe(FPresentListener);
                }
                FOnPresentCount++;
                FOnPresent += value;
            }
            remove
            {
                FOnPresent -= value;
                FOnPresentCount--;
                if (FOnPresentCount == 0)
                {
                    FGraphEventService.OnPresent.Unsubscribe(FPresentListener);
                }
            }
        }
        
        protected virtual void CallOnPresent(EventArgs e)
        {
            if (FOnPresent != null) 
            {
                FOnPresent(this, e);
            }
        }
        
        private int FOnDebugCount;
        private event EventHandler FOnDebug;
        public event EventHandler OnDebug
        {
            add
            {
                if (FOnDebugCount == 0)
                {
                    FGraphEventService.OnDebug.Subscribe(FDebugListener);
                }
                FOnDebugCount++;
                FOnDebug += value;
            }
            remove
            {
                FOnDebug -= value;
                FOnDebugCount--;
                if (FOnDebugCount == 0)
                {
                    FGraphEventService.OnDebug.Unsubscribe(FDebugListener);
                }
            }
        }
        
        protected virtual void CallOnDebug(EventArgs e)
        {
            if (FOnDebug != null) 
            {
                FOnDebug(this, e);
            }
        }
        
        private int FOnNetworkSyncCount;
        private event EventHandler FOnNetworkSync;
        public event EventHandler OnNetworkSync
        {
            add
            {
                if (FOnNetworkSyncCount == 0)
                {
                    FGraphEventService.OnNetworkSync.Subscribe(FNetworkSyncListener);
                }
                FOnNetworkSyncCount++;
                FOnNetworkSync += value;
            }
            remove
            {
                FOnNetworkSync -= value;
                FOnNetworkSyncCount--;
                if (FOnNetworkSyncCount == 0)
                {
                    FGraphEventService.OnNetworkSync.Unsubscribe(FNetworkSyncListener);
                }
            }
        }
        
        protected virtual void CallOnNetworkSync(EventArgs e)
        {
            if (FOnNetworkSync != null) 
            {
                FOnNetworkSync(this, e);
            }
        }
        
        private int FOnResetCacheCount;
        private event EventHandler FOnResetCache;
        public event EventHandler OnResetCache
        {
            add
            {
                if (FOnResetCacheCount == 0)
                {
                    FGraphEventService.OnResetCache.Subscribe(FResetCacheListener);
                }
                FOnResetCacheCount++;
                FOnResetCache += value;
            }
            remove
            {
                FOnResetCache -= value;
                FOnResetCacheCount--;
                if (FOnResetCacheCount == 0)
                {
                    FGraphEventService.OnResetCache.Unsubscribe(FResetCacheListener);
                }
            }
        }
        
        protected virtual void CallOnResetCache(EventArgs e)
        {
            if (FOnResetCache != null) 
            {
                FOnResetCache(this, e);
            }
        }
        
        public bool PresentUpFront
        {
            get
            {
                return FGraphEventService.PresentUpFront;
            }
        }
    }
}
