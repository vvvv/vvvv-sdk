using System;
using CefGlue;
using VVVV.Core;
using VVVV.Core.Logging;

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
            
            protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, out int screenX, out int screenY)
            {
                Shell.Instance.Logger.Log(LogType.Debug, string.Format("GetScreenPoint ({0}, {0})", viewX, viewY));
                return base.GetScreenPoint(browser, viewX, viewY, out screenX, out screenY);
            }
            
            protected override bool GetScreenRect(CefBrowser browser, out CefRect rect)
            {
                Shell.Instance.Logger.Log(LogType.Debug, string.Format("GetScreenRect"));
                return base.GetScreenRect(browser, out rect);
            }
            
            protected override bool GetViewRect(CefBrowser browser, out CefRect rect)
            {
                Shell.Instance.Logger.Log(LogType.Debug, string.Format("GetViewRect"));
                return base.GetViewRect(browser, out rect);
            }
            
            protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRect[] dirtyRects, IntPtr buffer)
            {
                int width, height;
                
                switch (type) {
                    case CefPaintElementType.View:
                        browser.GetSize(CefPaintElementType.View, out width, out height);
                        FRenderer.Paint(dirtyRects, buffer, width * 4);
                        break;
                    case CefPaintElementType.Popup:
                        
                        break;
                }
                base.OnPaint(browser, type, dirtyRects, buffer);
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
                FRenderer.FErrorText = string.Empty;
                FRenderer.Attach(browser);
                base.OnAfterCreated(browser);
            }
            
            protected override void OnBeforeClose(CefBrowser browser)
            {
                FRenderer.Detach();
                base.OnBeforeClose(browser);
            }
        }
        
//        class DomEventEventListener : CefDomEventListener
//        {
//            protected override void HandleEvent(CefDomEvent e)
//            {
//                var currentTarget = e.GetCurrentTarget();
//                var target = e.GetTarget();
//            }
//        }
//        
//        class DomVisitor : CefDomVisitor
//        {
//            protected override void Visit(CefDomDocument document)
//            {
//                var rootNode = document.GetDocument();
//                rootNode.AddEventListener("mouseover", new DomEventEventListener(), false);
//            }
//        }
        
        class LoadHandler : CefLoadHandler
        {
            private readonly WebRenderer FRenderer;
            
            public LoadHandler(WebRenderer renderer)
            {
                FRenderer = renderer;
            }
            
            protected override void OnLoadStart(CefBrowser browser, CefFrame frame)
            {
                FRenderer.FFrameLoadCount++;
                FRenderer.FErrorText = string.Empty;
                base.OnLoadStart(browser, frame);
            }
            
            protected override bool OnLoadError(CefBrowser browser, CefFrame frame, CefHandlerErrorCode errorCode, string failedUrl, ref string errorText)
            {
                FRenderer.FFrameLoadCount = 0;
                FRenderer.FErrorText = errorText;
                return base.OnLoadError(browser, frame, errorCode, failedUrl, ref errorText);
            }
            
            protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
            {
                FRenderer.FFrameLoadCount--;
                FRenderer.FErrorText = string.Empty;
                base.OnLoadEnd(browser, frame, httpStatusCode);
//                frame.VisitDom(new DomVisitor());
            }
        }
        
        private readonly CefRenderHandler FRenderHandler;
        private readonly CefLifeSpanHandler FLifeSpanHandler;
        private readonly CefLoadHandler FLoadHandler;
        
        public WebClient(WebRenderer renderer)
        {
            FRenderHandler = new RenderHandler(renderer);
            FLifeSpanHandler = new LifeSpanHandler(renderer);
            FLoadHandler = new LoadHandler(renderer);
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
        
        protected override CefV8ContextHandler GetV8ContextHandler()
        {
            return base.GetV8ContextHandler();
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
            return FLoadHandler;
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
