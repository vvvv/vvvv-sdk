using System;
using CefGlue;

namespace VVVV.Nodes.HTML
{
    public class WebClient : CefClient
    {
        class RenderHandler : CefRenderHandler
        {
            private readonly WebRenderer FRenderer;
            
            public RenderHandler(WebRenderer renderer)
            {
                FRenderer = renderer;
            }
            
            protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRect dirtyRect, IntPtr buffer)
            {
                int width, height;
                
                switch (type) {
                    case CefPaintElementType.View:
                        browser.GetSize(CefPaintElementType.View, out width, out height);
                        FRenderer.Paint(dirtyRect, buffer, width * 4);
                        break;
                    case CefPaintElementType.Popup:
                        
                        break;
                }
                base.OnPaint(browser, type, dirtyRect, buffer);
            }
        }
        
        class LifeSpanHandler : CefLifeSpanHandler
        {
            private readonly WebRenderer FRenderer;
            
            public LifeSpanHandler(WebRenderer renderer)
            {
                FRenderer = renderer;
            }
            
            protected override void OnAfterCreated(CefBrowser browser)
            {
                FRenderer.Attach(browser);
                base.OnAfterCreated(browser);
            }
            
            protected override void OnBeforeClose(CefBrowser browser)
            {
                FRenderer.Detach();
                base.OnBeforeClose(browser);
            }
        }
        
        private readonly CefRenderHandler FRenderHandler;
        private readonly CefLifeSpanHandler FLifeSpanHandler;
        
        public WebClient(WebRenderer renderer)
        {
            FRenderHandler = new RenderHandler(renderer);
            FLifeSpanHandler = new LifeSpanHandler(renderer);
        }
        
        protected override CefDisplayHandler GetDisplayHandler()
        {
            return base.GetDisplayHandler();
        }
        
        protected override CefDragHandler GetDragHandler()
        {
            return base.GetDragHandler();
        }
        
        protected override CefFindHandler GetFindHandler()
        {
            return base.GetFindHandler();
        }
        
        protected override CefFocusHandler GetFocusHandler()
        {
            return base.GetFocusHandler();
        }
        
        protected override CefJSBindingHandler GetJSBindingHandler()
        {
            return base.GetJSBindingHandler();
        }
        
        protected override CefJSDialogHandler GetJSDialogHandler()
        {
            return base.GetJSDialogHandler();
        }
        
        protected override CefKeyboardHandler GetKeyboardHandler()
        {
            return base.GetKeyboardHandler();
        }
        
        protected override CefLifeSpanHandler GetLifeSpanHandler()
        {
            return FLifeSpanHandler;
        }
        
        protected override CefLoadHandler GetLoadHandler()
        {
            return base.GetLoadHandler();
        }
        
        protected override CefMenuHandler GetMenuHandler()
        {
            return base.GetMenuHandler();
        }
        
        protected override CefPrintHandler GetPrintHandler()
        {
            return base.GetPrintHandler();
        }
        
        protected override CefRenderHandler GetRenderHandler()
        {
            return FRenderHandler;
        }
        
        protected override CefRequestHandler GetRequestHandler()
        {
            return base.GetRequestHandler();
        }
    }
}
