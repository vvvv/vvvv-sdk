using System;
using System.Runtime.InteropServices;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
    [ComVisible(false)]
    class MainLoop : IMainLoop
    {
        class InitFrameListener : IInternalMainLoopEventListener
        {
            private readonly MainLoop FMainLoop;

            public InitFrameListener(MainLoop mainLoop)
            {
                FMainLoop = mainLoop;
            }

            public void HandleEvent()
            {
                FMainLoop.CallOnInitFrame(EventArgs.Empty);
            }
        }

        class PrepareGraphListener : IInternalMainLoopEventListener
        {
            private readonly MainLoop FMainLoop;
            
            public PrepareGraphListener(MainLoop mainLoop)
            {
                FMainLoop = mainLoop;
            }
            
            public void HandleEvent()
            {
                FMainLoop.CallOnPrepareGraph(EventArgs.Empty);
            }
        }
        
        class UpdateViewListener : IInternalMainLoopEventListener
        {
            private readonly MainLoop FMainLoop;
            
            public UpdateViewListener(MainLoop mainLoop)
            {
                FMainLoop = mainLoop;
            }
            
            public void HandleEvent()
            {
                FMainLoop.CallOnUpdateView(EventArgs.Empty);
            }
        }
        
        class RenderListener : IInternalMainLoopEventListener
        {
            private readonly MainLoop FMainLoop;
            
            public RenderListener(MainLoop mainLoop)
            {
                FMainLoop = mainLoop;
            }
            
            public void HandleEvent()
            {
                FMainLoop.CallOnRender(EventArgs.Empty);
            }
        }
        
        class PresentListener : IInternalMainLoopEventListener
        {
            private readonly MainLoop FMainLoop;
            
            public PresentListener(MainLoop mainLoop)
            {
                FMainLoop = mainLoop;
            }
            
            public void HandleEvent()
            {
                FMainLoop.CallOnPresent(EventArgs.Empty);
            }
        }
        
        class DebugListener : IInternalMainLoopEventListener
        {
            private readonly MainLoop FMainLoop;
            
            public DebugListener(MainLoop mainLoop)
            {
                FMainLoop = mainLoop;
            }
            
            public void HandleEvent()
            {
                FMainLoop.CallOnDebug(EventArgs.Empty);
            }
        }
        
        class NetworkSyncListener : IInternalMainLoopEventListener
        {
            private readonly MainLoop FMainLoop;
            
            public NetworkSyncListener(MainLoop mainLoop)
            {
                FMainLoop = mainLoop;
            }
            
            public void HandleEvent()
            {
                FMainLoop.CallOnNetworkSync(EventArgs.Empty);
            }
        }
        
        class ResetCacheListener : IInternalMainLoopEventListener
        {
            private readonly MainLoop FMainLoop;
            
            public ResetCacheListener(MainLoop mainLoop)
            {
                FMainLoop = mainLoop;
            }
            
            public void HandleEvent()
            {
                FMainLoop.CallOnResetCache(EventArgs.Empty);
            }
        }
        
        private readonly IInternalMainLoop FMainLoop;
        private readonly InitFrameListener FInitFrameListener;
        private readonly PrepareGraphListener FPrepareGraphListener;
        private readonly UpdateViewListener FUpdateViewListener;
        private readonly RenderListener FRenderListener;
        private readonly PresentListener FPresentListener;
        private readonly DebugListener FDebugListener;
        private readonly NetworkSyncListener FNetworkSyncListener;
        private readonly ResetCacheListener FResetCacheListener;
        
        public MainLoop(IInternalMainLoop mainLoop)
        {
            FMainLoop = mainLoop;
            FInitFrameListener = new InitFrameListener(this);
            FPrepareGraphListener = new PrepareGraphListener(this);
            FUpdateViewListener = new UpdateViewListener(this);
            FRenderListener = new RenderListener(this);
            FPresentListener = new PresentListener(this);
            FDebugListener = new DebugListener(this);
            FNetworkSyncListener = new NetworkSyncListener(this);
            FResetCacheListener = new ResetCacheListener(this);
        }
        
        private int FOnInitFrameCount;
        private event EventHandler FOnInitFrame;
        public event EventHandler OnInitFrame
        {
            add
            {
                if (FOnInitFrameCount == 0)
                {
                    FMainLoop.OnInitFrame.Subscribe(FInitFrameListener);
                }
                FOnInitFrameCount++;
                FOnInitFrame += value;
            }
            remove
            {
                FOnInitFrame -= value;
                FOnInitFrameCount--;
                if (FOnInitFrameCount == 0)
                {
                    FMainLoop.OnInitFrame.Unsubscribe(FInitFrameListener);
                }
            }
        }

        protected virtual void CallOnInitFrame(EventArgs e)
        {
            if (FOnInitFrame != null)
            {
                FOnInitFrame(this, e);
            }
        }

        private int FOnPrepareGraphCount;
        private event EventHandler FOnPrepareGraph;
        public event EventHandler OnPrepareGraph
        {
            add
            {
                if (FOnPrepareGraphCount == 0)
                {
                    FMainLoop.OnPrepareGraph.Subscribe(FPrepareGraphListener);
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
                    FMainLoop.OnPrepareGraph.Unsubscribe(FPrepareGraphListener);
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
                    FMainLoop.OnUpdateView.Subscribe(FUpdateViewListener);
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
                    FMainLoop.OnUpdateView.Unsubscribe(FUpdateViewListener);
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
                    FMainLoop.OnRender.Subscribe(FRenderListener);
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
                    FMainLoop.OnRender.Unsubscribe(FRenderListener);
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
                    FMainLoop.OnPresent.Subscribe(FPresentListener);
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
                    FMainLoop.OnPresent.Unsubscribe(FPresentListener);
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
                    FMainLoop.OnDebug.Subscribe(FDebugListener);
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
                    FMainLoop.OnDebug.Unsubscribe(FDebugListener);
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
                    FMainLoop.OnNetworkSync.Subscribe(FNetworkSyncListener);
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
                    FMainLoop.OnNetworkSync.Unsubscribe(FNetworkSyncListener);
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
                    FMainLoop.OnResetCache.Subscribe(FResetCacheListener);
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
                    FMainLoop.OnResetCache.Unsubscribe(FResetCacheListener);
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
                return FMainLoop.PresentUpFront;
            }
        }
    }
}
