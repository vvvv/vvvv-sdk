using System;
using Xilium.CefGlue;
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

            protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
            {
                rect.X = rect.Y = 0;
                rect.Width = FRenderer.Size.Width;
                rect.Height = FRenderer.Size.Height;
                return true;
            }

            protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect)
            {
                rect.X = rect.Y = 0;
                rect.Width = FRenderer.Size.Width;
                rect.Height = FRenderer.Size.Height;
                return true;
            }

            protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
            {
                return base.GetScreenPoint(browser, viewX, viewY, ref screenX, ref screenY);
            }

            protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle)
            {
                
            }

            protected override void OnPopupShow(CefBrowser browser, bool show)
            {
                base.OnPopupShow(browser, show);
            }

            protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
            {

            }

            protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
            {
                switch (type) {
                    case CefPaintElementType.View:
                        FRenderer.Paint(dirtyRects, buffer, width * 4);
                        break;
                    case CefPaintElementType.Popup:
                        break;
                }
            }

            protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
            {
                return false;
            }

            protected override void OnScrollOffsetChanged(CefBrowser browser)
            {
            }
        }
        
        class LifeSpanHandler : CefLifeSpanHandler
        {
            private readonly WebClient FWebClient;
            private readonly HTMLTextureRenderer FRenderer;
            
            public LifeSpanHandler(WebClient webClient, HTMLTextureRenderer renderer)
            {
                FWebClient = webClient;
                FRenderer = renderer;
            }

            protected override bool OnBeforePopup(CefBrowser browser, CefFrame frame, string targetUrl, string targetFrameName, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo, ref CefClient client, CefBrowserSettings settings, ref bool noJavascriptAccess)
            {
                FRenderer.LoadUrl(targetUrl);
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
                base.OnBeforeClose(browser);
            }
        }

        class DomEventEventListener : CefDomEventListener
        {
            protected override void HandleEvent(CefDomEvent e)
            {
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

            protected override void OnLoadError(CefBrowser browser, CefFrame frame, CefErrorCode errorCode, string errorText, string failedUrl)
            {
                FRenderer.OnLoadError(frame, errorCode, failedUrl, errorText);
                base.OnLoadError(browser, frame, errorCode, errorText, failedUrl);
            }
            
            protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
            {
                FRenderer.OnLoadEnd(frame, httpStatusCode);
                base.OnLoadEnd(browser, frame, httpStatusCode);
            }
        }

        class KeyboardHandler : CefKeyboardHandler
        {
            protected override bool OnKeyEvent(CefBrowser browser, CefKeyEvent keyEvent, IntPtr osEvent)
            {
                return base.OnKeyEvent(browser, keyEvent, osEvent);
            }

            protected override bool OnPreKeyEvent(CefBrowser browser, CefKeyEvent keyEvent, IntPtr os_event, out bool isKeyboardShortcut)
            {
                return base.OnPreKeyEvent(browser, keyEvent, os_event, out isKeyboardShortcut);
            }
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
                FRenderer.Logger.Log(LogType.Message, string.Format("{0} ({1}:{2})", message, source, line));
                return base.OnConsoleMessage(browser, message, source, line);
            }
        }

        private readonly HTMLTextureRenderer FRenderer;
        private readonly CefRenderHandler FRenderHandler;
        private readonly CefLifeSpanHandler FLifeSpanHandler;
        private readonly CefLoadHandler FLoadHandler;
        private readonly CefKeyboardHandler FKeyboardHandler;
        private readonly CefDisplayHandler FDisplayHandler;
        
        public WebClient(HTMLTextureRenderer renderer)
        {
            FRenderer = renderer;
            FRenderHandler = new RenderHandler(renderer);
            FLifeSpanHandler = new LifeSpanHandler(this, renderer);
            FLoadHandler = new LoadHandler(renderer);
            FKeyboardHandler = new KeyboardHandler();
            FDisplayHandler = new DisplayHandler(renderer);
        }
        
        protected override CefDisplayHandler GetDisplayHandler()
        {
            return FDisplayHandler;
        }
        
        protected override CefFocusHandler GetFocusHandler()
        {
            return base.GetFocusHandler();
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
        
        protected override CefRenderHandler GetRenderHandler()
        {
            return FRenderHandler;
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message)
        {
            switch (message.Name)
            {
                case "dom-response":
                    var identifier = message.GetFrameIdentifier();
                    var frame = browser.GetFrame(identifier);
                    if (frame != null)
                    {
                        var arguments = message.Arguments;
                        var success = arguments.GetBool(2);
                        var s = arguments.GetString(3);
                        if (success)
                        {
                            var dom = XDocument.Parse(s);
                            FRenderer.OnUpdateDom(dom);
                        }
                        else
                            FRenderer.OnError(s);
                    }
                    return true;
                default:
                    break;
            }
            return base.OnProcessMessageReceived(browser, sourceProcess, message);
        }
    }
}
