using System;
using CefGlue;
using VVVV.Core;
using VVVV.Core.Logging;
using System.Xml.Linq;

namespace VVVV.Nodes.Texture.HTML
{
    public class WebClient : CefClient
    {
        class RenderHandler : CefRenderHandler
        {
            private readonly HTMLTextureRenderer FRenderer;
            
            public RenderHandler(HTMLTextureRenderer renderer)
            {
                FRenderer = renderer;
            }
            
            protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, out int screenX, out int screenY)
            {
                return base.GetScreenPoint(browser, viewX, viewY, out screenX, out screenY);
            }
            
            protected override bool GetScreenRect(CefBrowser browser, out CefRect rect)
            {
                return base.GetScreenRect(browser, out rect);
            }
            
            protected override bool GetViewRect(CefBrowser browser, out CefRect rect)
            {
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
            private readonly HTMLTextureRenderer FRenderer;
            
            public LifeSpanHandler(HTMLTextureRenderer renderer)
            {
                FRenderer = renderer;
            }

            protected override bool OnBeforePopup(CefBrowser parentBrowser, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo, string url, ref CefClient client, CefBrowserSettings settings)
            {
                // We do not support popups
                FRenderer.DoLoadURL(url);
                return true;
            }
            
            protected override void OnAfterCreated(CefBrowser browser)
            {
                FRenderer.Attach(browser);
                base.OnAfterCreated(browser);
            }
            
            protected override void OnBeforeClose(CefBrowser browser)
            {
                FRenderer.Detach();
                browser.Dispose();
                base.OnBeforeClose(browser);
            }
        }
        
        class LoadHandler : CefLoadHandler
        {
            private readonly HTMLTextureRenderer FRenderer;
            
            public LoadHandler(HTMLTextureRenderer renderer)
            {
                FRenderer = renderer;
            }
            
            protected override void OnLoadStart(CefBrowser browser, CefFrame frame)
            {
                FRenderer.OnLoadStart(frame);
                base.OnLoadStart(browser, frame);
            }
            
            protected override bool OnLoadError(CefBrowser browser, CefFrame frame, CefHandlerErrorCode errorCode, string failedUrl, ref string errorText)
            {
                FRenderer.OnLoadError(frame, errorCode, failedUrl, errorText);
                return base.OnLoadError(browser, frame, errorCode, failedUrl, ref errorText);
            }
            
            protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
            {
                FRenderer.OnLoadEnd(frame, httpStatusCode);
                base.OnLoadEnd(browser, frame, httpStatusCode);
            }
        }

        class KeyboardHandler : CefKeyboardHandler
        {
            protected override bool OnKeyEvent(CefBrowser browser, CefHandlerKeyEventType type, int code, CefHandlerKeyEventModifiers modifiers, bool isSystemKey, bool isAfterJavaScript)
            {
                return base.OnKeyEvent(browser, type, code, modifiers, isSystemKey, isAfterJavaScript);
            }
        }

        class RequestHandler : CefRequestHandler
        {
            protected override bool OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, CefHandlerNavType navType, bool isRedirect)
            {
                return base.OnBeforeBrowse(browser, frame, request, navType, isRedirect);
            }
        }

        class V8Handler : CefV8Handler
        {
            
        }

        class PrintHandler : CefPrintHandler
        {
            
        }

        class DisplayHandler : CefDisplayHandler
        {
            private readonly HTMLTextureRenderer FRenderer;

            public DisplayHandler(HTMLTextureRenderer renderer)
            {
                FRenderer = renderer;
            }

            protected override bool OnConsoleMessage(CefBrowser browser, string message, string source, int line)
            {
                return base.OnConsoleMessage(browser, message, source, line);
            }
        }
        
        private readonly CefRenderHandler FRenderHandler;
        private readonly CefLifeSpanHandler FLifeSpanHandler;
        private readonly CefLoadHandler FLoadHandler;
        private readonly CefKeyboardHandler FKeyboardHandler;
        private readonly CefRequestHandler FRequestHandler;
        private readonly CefDisplayHandler FDisplayHandler;
        
        public WebClient(HTMLTextureRenderer renderer)
        {
            FRenderHandler = new RenderHandler(renderer);
            FLifeSpanHandler = new LifeSpanHandler(renderer);
            FLoadHandler = new LoadHandler(renderer);
            FKeyboardHandler = new KeyboardHandler();
            FRequestHandler = new RequestHandler();
            FDisplayHandler = new DisplayHandler(renderer);
        }
        
        protected override CefDisplayHandler GetDisplayHandler()
        {
            return FDisplayHandler;
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
            return FKeyboardHandler;
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
            return FRequestHandler;
        }
    }
}
