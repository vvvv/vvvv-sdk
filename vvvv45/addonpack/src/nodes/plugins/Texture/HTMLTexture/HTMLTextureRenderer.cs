using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xilium.CefGlue;
using SlimDX.Direct3D9;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VMath;
using System.Text;
using System.Globalization;
using VVVV.Utils.IO;
using System.Xml.Linq;
using EX9 = SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V2;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using VVVV.Utils.Win32;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace VVVV.Nodes.Texture.HTML
{
    public class HTMLTextureRenderer : IDisposable
    {
        public const string DEFAULT_URL = "http://vvvv.org";
        public const string DEFAULT_CONTENT = @"<html><head></head><body bgcolor=""#ffffff""></body></html>";
        public const int DEFAULT_WIDTH = 640;
        public const int DEFAULT_HEIGHT = 480;
        public const int MIN_FRAME_RATE = 1;
        public const int MAX_FRAME_RATE = 60;

        private volatile bool FEnabled;
        private readonly WebClient FWebClient;
        private CefBrowser FBrowser;
        private CefRequestContext FRequestContext;
        private CefBrowserHost FBrowserHost;
        private string FUrl;
        private string FHtml;
        private bool FDomIsValid;
        private XDocument FCurrentDom;
        private string FCurrentUrl;
        private string FErrorText;
        private Size FSize;
        public ILogger Logger;
        private readonly AutoResetEvent FBrowserAttachedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent FBrowserDetachedEvent = new AutoResetEvent(false);

        /// <summary>
        /// Create a new texture renderer.
        /// </summary>
        /// <param name="logger">The logger to log to.</param>
        /// <param name="frameRate">
        /// The maximum rate in frames per second (fps) that CefRenderHandler::OnPaint will
        /// be called for a windowless browser. The actual fps may be lower if the browser
        /// cannot generate frames at the requested rate. The minimum value is 1 and the 
        /// maximum value is 60 (default 30).
        /// </param>
        public HTMLTextureRenderer(ILogger logger, int frameRate)
        {
            Logger = logger;
            FrameRate = VMath.Clamp(frameRate, MIN_FRAME_RATE, MAX_FRAME_RATE);

            FLoaded = false;

            var settings = new CefBrowserSettings();
            settings.FileAccessFromFileUrls = CefState.Enabled;
            settings.Plugins = CefState.Enabled;
            settings.RemoteFonts = CefState.Enabled;
            settings.UniversalAccessFromFileUrls = CefState.Enabled;
            settings.WebGL = CefState.Enabled;
            settings.WebSecurity = CefState.Disabled;
            settings.WindowlessFrameRate = frameRate;

            var windowInfo = CefWindowInfo.Create();
            windowInfo.SetAsWindowless(IntPtr.Zero, true);

            FWebClient = new WebClient(this);
            // See http://magpcss.org/ceforum/viewtopic.php?f=6&t=5901
            // We need to maintain different request contexts in order to have different zoom levels
            // See https://bitbucket.org/chromiumembedded/cef/issues/1314
            var rcSettings = new CefRequestContextSettings()
            {
                IgnoreCertificateErrors = true
            };
            FRequestContext = CefRequestContext.CreateContext(rcSettings, new WebClient.RequestContextHandler());
            CefBrowserHost.CreateBrowser(windowInfo, FWebClient, settings, FRequestContext);
            // Block until browser is created
            FBrowserAttachedEvent.WaitOne();
        }

        public int FrameRate { get; private set; }

        internal void Attach(CefBrowser browser)
        {
            FBrowser = browser;
            FBrowserHost = browser.GetHost();
            FBrowserHost.SetMouseCursorChangeDisabled(true);
            FBrowserAttachedEvent.Set();
        }

        internal void Detach()
        {
            FBrowser.Dispose();
            FBrowserDetachedEvent.Set();
        }

        public void Dispose()
        {
            FBrowserHost.CloseBrowser(true);
            FBrowserDetachedEvent.WaitOne();
            FBrowserAttachedEvent.Dispose();
            FBrowserDetachedEvent.Dispose();
            FRequestContext.Dispose();
            if (FMouseSubscription != null)
            {
                FMouseSubscription.Dispose();
                FMouseSubscription = null;
            }
            if (FKeyboardSubscription != null)
            {
                FKeyboardSubscription.Dispose();
                FKeyboardSubscription = null;
            }
            DestroyResources();
        }

        public void LoadUrl(string url)
        {
            LoadUrl(FSize, url);
        }

        public void LoadUrl(Size size, string url)
        {
            // Normalize inputs
            url = string.IsNullOrEmpty(url) ? "about:blank" : url;
            if (FUrl != url || FSize != size)
            {
                // Set new values
                FUrl = url;
                FHtml = null;
                Size = size;
                // Reset all computed values
                Reset();
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    // Create a valid url
                    var uri = new UriBuilder(FUrl).Uri;
                    mainFrame.LoadUrl(uri.ToString());
                }
            }
        }

        public void LoadString(Size size, string html, string baseUrl, string patchPath)
        {
            // Normalize inputs
            baseUrl = string.IsNullOrEmpty(baseUrl) ? patchPath : baseUrl;
            html = html ?? string.Empty;
            if (FUrl != baseUrl || FHtml != html || FSize != size)
            {
                // Set new values
                FUrl = baseUrl;
                FHtml = html;
                Size = size;
                // Reset all computed values
                Reset();
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    // Create a valid url
                    var uri = new UriBuilder(baseUrl).Uri;
                    if (uri.IsFile && !uri.ToString().EndsWith("/"))
                        // Append trailing slash
                        uri = new UriBuilder(uri.ToString() + "/").Uri;
                    mainFrame.LoadUrl(baseUrl);
                }
            }
        }

        private void Reset()
        {
            FLoaded = false;
            FDocumentSizeIsValid = false;
            FDomIsValid = false;
            FErrorText = string.Empty;
            if (IsAutoSize)
                FBrowserHost.WasResized();
        }

        public void ExecuteJavaScript(string javaScript)
        {
            using (var mainFrame = FBrowser.GetMainFrame())
            {
                mainFrame.ExecuteJavaScript(javaScript, string.Empty, 0);
            }
        }

        public void UpdateDom()
        {
            using (var frame = FBrowser.GetMainFrame())
                UpdateDom(frame);
        }

        private void UpdateDom(CefFrame frame)
        {
            var request = CefProcessMessage.Create("dom-request");
            request.SetFrameIdentifier(frame.Identifier);
            FBrowser.SendProcessMessage(CefProcessId.Renderer, request);
        }

        internal void OnUpdateDom(XDocument dom)
        {
            FCurrentDom = dom;
            FDomIsValid = true;
            FErrorText = null;
        }

        internal void OnUpdateDom(string error)
        {
            FCurrentDom = null;
            FDomIsValid = true;
            FErrorText = error;
        }

        public void UpdateDocumentSize()
        {
            using (var frame = FBrowser.GetMainFrame())
            {
                var request = CefProcessMessage.Create("document-size-request");
                request.SetFrameIdentifier(frame.Identifier);
                FBrowser.SendProcessMessage(CefProcessId.Renderer, request);
            }
        }

        internal void OnDocumentSize(CefFrame frame, int width, int height)
        {
            if (frame.IsMain)
            {
                // Retrieve the current size
                var size = Size;
                // Set the new values
                FDocumentSize = new Size(width, height);
                FDocumentSizeIsValid = true;
                // Notify the browser about the change in case the size was affected
                // by this change
                var newSize = Size;
                if (IsAutoSize && size != newSize)
                {
                    // Put all textures in the degraded state
                    lock (FTextures)
                    {
                        for (int i = 0; i < FTextures.Count; i++)
                            if (FTextures[i].Size != newSize)
                                FTextures[i] = FTextures[i].Update(newSize);
                    }
                    FBrowserHost.WasResized();
                    FBrowserHost.Invalidate(CefPaintElementType.View);
                }
            }
        }

        public void Reload()
        {
            FBrowser.Reload();
        }

        public XDocument CurrentDom
        {
            get { return FCurrentDom; }
        }

        public string CurrentUrl
        {
            get { return FCurrentUrl; }
        }

        public string CurrentError
        {
            get { return FErrorText; }
        }

        public string CurrentHTML
        {
            get { return FHtml; }
        }

        public Size Size
        {
            get
            {
                var size = FSize;
                if (IsAutoWidth)
                    size.Width = FDocumentSizeIsValid ? FDocumentSize.Width : 0;
                if (IsAutoHeight)
                    size.Height = FDocumentSizeIsValid ? FDocumentSize.Height : 0;
                return size;
            }
            set
            {
                if (value != FSize)
                {
                    FSize = value;
                    if (!IsAutoSize)
                        FBrowserHost.WasResized();
                }
            }
        }

        public bool IsAutoWidth { get { return FSize.Width <= 0; } }
        public bool IsAutoHeight { get { return FSize.Height <= 0; } }
        public bool IsAutoSize { get { return IsAutoWidth || IsAutoHeight; } }

        private bool FDocumentSizeIsValid;
        private Size FDocumentSize;
        public Size DocumentSize
        {
            get { return FDocumentSize; }
        }

        private double FZoomLevel;
        public double ZoomLevel
        {
            get { return FZoomLevel; }
            set
            {
                if (FZoomLevel != value)
                {
                    FZoomLevel = value;
                    FBrowserHost.SetZoomLevel(value);
                }
            }
        }

        private Vector2D FScrollTo;
        public Vector2D ScrollTo
        {
            get { return FScrollTo; }
            set
            {
                if (FScrollTo != value)
                {
                    using (var mainFrame = FBrowser.GetMainFrame())
                    {
                        var x = VMath.Map(value.x, 0, 1, 0, 1, TMapMode.Clamp);
                        var y = VMath.Map(value.y, 0, 1, 0, 1, TMapMode.Clamp);
                        mainFrame.ExecuteJavaScript(
                            string.Format(CultureInfo.InvariantCulture,
                                @"
                            var body = document.body,
                                html = document.documentElement;
                            var width = Math.max(body.scrollWidth, body.offsetWidth, html.clientWidth, html.scrollWidth, html.offsetWidth);
                            var height = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight);
                            window.scrollTo({0} *  width, {1} * height);
                            ",
                                 x,
                                 y
                            ),
                            string.Empty,
                            0);
                    }
                    FScrollTo = value;
                }
            }
        }

        private Subscription<Mouse, MouseNotification> FMouseSubscription;
        private int FMouseWheel;
        private int FClickCount;
        private MouseButtons FLastClickButton;
        private Point FLastClickPosition;
        private Stopwatch FStopwatch;
        public Mouse Mouse
        {
            set
            {
                if (FMouseSubscription == null)
                {
                    FStopwatch = Stopwatch.StartNew();
                    FMouseSubscription = new Subscription<Mouse, MouseNotification>(
                        mouse => mouse.MouseNotifications,
                        (mouse, n) =>
                        {
                            var mouseEvent = new CefMouseEvent(
                                (int)VMath.Map(n.Position.X, 0, n.ClientArea.Width, 0, Size.Width, TMapMode.Clamp),
                                (int)VMath.Map(n.Position.Y, 0, n.ClientArea.Height, 0, Size.Height, TMapMode.Clamp),
                                GetMouseModifiers(mouse.PressedButtons));
                            switch (n.Kind)
                            {
                                case MouseNotificationKind.MouseDown:
                                case MouseNotificationKind.MouseUp:
                                    var mouseButtonNotification = n as MouseButtonNotification;
                                    CefMouseButtonType cefButtons;
                                    switch (mouseButtonNotification.Buttons)
                                    {
                                        case MouseButtons.Left:
                                            cefButtons = CefMouseButtonType.Left;
                                            break;
                                        case MouseButtons.Middle:
                                            cefButtons = CefMouseButtonType.Middle;
                                            break;
                                        case MouseButtons.Right:
                                            cefButtons = CefMouseButtonType.Right;
                                            break;
                                        default:
                                            cefButtons = CefMouseButtonType.Left;
                                            break;
                                    }

                                    switch (n.Kind)
                                    {
                                        case MouseNotificationKind.MouseDown:
                                            // Manage the click count
                                            var elapsedTime = FStopwatch.ElapsedMilliseconds;
                                            FStopwatch.Restart();
                                            if (FLastClickButton == mouseButtonNotification.Buttons &&
                                                elapsedTime <= SystemInformation.DoubleClickTime &&
                                                Math.Abs(FLastClickPosition.X - mouseButtonNotification.Position.X) <= SystemInformation.DoubleClickSize.Width &&
                                                Math.Abs(FLastClickPosition.Y - mouseButtonNotification.Position.Y) <= SystemInformation.DoubleClickSize.Height)
                                                FClickCount++;
                                            else
                                                FClickCount = 1;

                                            FBrowserHost.SendMouseClickEvent(mouseEvent, cefButtons, false, FClickCount);
                                            break;
                                        case MouseNotificationKind.MouseUp:
                                            FBrowserHost.SendMouseClickEvent(mouseEvent, cefButtons, true, FClickCount);
                                            break;
                                    }

                                    FLastClickPosition = mouseButtonNotification.Position;
                                    FLastClickButton = mouseButtonNotification.Buttons;
                                    break;
                                case MouseNotificationKind.MouseClick:
                                    // TODO: Use this event directly and remove above code.
                                    break;
                                case MouseNotificationKind.MouseMove:
                                    FBrowserHost.SendMouseMoveEvent(mouseEvent, false);
                                    break;
                                case MouseNotificationKind.MouseWheel:
                                    var mouseWheel = n as MouseWheelNotification;
                                    var wheel = FMouseWheel;
                                    FMouseWheel += mouseWheel.WheelDelta;
                                    var delta = (int)Math.Round((float)(FMouseWheel - wheel) / Const.WHEEL_DELTA);
                                    FBrowserHost.SendMouseWheelEvent(mouseEvent, 0, mouseWheel.WheelDelta);
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    );
                }
                FMouseSubscription.Update(value);
            }
        }

        private static CefEventFlags GetMouseModifiers(MouseButtons buttons)
        {
            var result = CefEventFlags.None;
            if ((buttons & MouseButtons.Left) != 0)
                result |= CefEventFlags.LeftMouseButton;
            if ((buttons & MouseButtons.Middle) != 0)
                result |= CefEventFlags.MiddleMouseButton;
            if ((buttons & MouseButtons.Right) != 0)
                result |= CefEventFlags.RightMouseButton;
            return result;
        }

        private Subscription<Keyboard, KeyNotification> FKeyboardSubscription;
        private Keyboard FKeyboard;
        public Keyboard Keyboard
        {
            set
            {
                if (FKeyboardSubscription == null)
                    FKeyboardSubscription = new Subscription<Keyboard, KeyNotification>(
                        keyboard => keyboard.KeyNotifications,
                        (keyboard, n) =>
                        {
                            var keyEvent = new CefKeyEvent()
                            {
                                Modifiers = (CefEventFlags)((int)(FKeyboard.Modifiers) >> 15)
                            };
                            switch (n.Kind)
                            {
                                case KeyNotificationKind.KeyDown:
                                    var keyDown = n as KeyDownNotification;
                                    keyEvent.EventType = CefKeyEventType.KeyDown;
                                    keyEvent.WindowsKeyCode = (int)keyDown.KeyCode;
                                    keyEvent.NativeKeyCode = (int)keyDown.KeyCode;
                                    break;
                                case KeyNotificationKind.KeyPress:
                                    var keyPress = n as KeyPressNotification;
                                    keyEvent.EventType = CefKeyEventType.Char;
                                    keyEvent.Character = keyPress.KeyChar;
                                    keyEvent.UnmodifiedCharacter = keyPress.KeyChar;
                                    keyEvent.WindowsKeyCode = (int)keyPress.KeyChar;
                                    keyEvent.NativeKeyCode = (int)keyPress.KeyChar;
                                    break;
                                case KeyNotificationKind.KeyUp:
                                    var keyUp = n as KeyUpNotification;
                                    keyEvent.EventType = CefKeyEventType.KeyUp;
                                    keyEvent.WindowsKeyCode = (int)keyUp.KeyCode;
                                    keyEvent.NativeKeyCode = (int)keyUp.KeyCode;
                                    break;
                                default:
                                    break;
                            }
                            FBrowserHost.SendKeyEvent(keyEvent);
                        }
                    );
                FKeyboard = value;
                FKeyboardSubscription.Update(value);
            }
        }

        private bool FIsLoading;
        public bool IsLoading
        {
            get
            {
                if (FIsLoading || !FDomIsValid || (IsAutoSize && !FDocumentSizeIsValid))
                    return true;
                lock (FTextures)
                {
                    return FTextures.Any(t => !t.IsValid);
                }
            }
        }

        private bool FLoaded;
        public bool Loaded
        {
            get { return FLoaded; }
        }

        public bool Enabled
        {
            get { return FEnabled; }
            set
            {
                if (FEnabled != value)
                {
                    if (FEnabled)
                    {
                        FBrowser.StopLoad();
                        FBrowserHost.WasHidden(true);
                    }
                    FEnabled = value;
                    if (FEnabled)
                    {
                        FBrowserHost.WasHidden(false);
                        FBrowserHost.Invalidate(CefPaintElementType.View);
                    }
                }
            }
        }

        internal void OnLoadError(CefFrame frame, CefErrorCode errorCode, string failedUrl, string errorText)
        {
            if (frame.IsMain)
            {
                FCurrentDom = null;
                FDomIsValid = true;
                FErrorText = errorText;
            }
        }

        internal void OnLoadingStateChange(bool isLoading, bool canGoBack, bool canGoForward)
        {
            FIsLoading = isLoading;
            if (!isLoading)
            {
                using (var frame = FBrowser.GetMainFrame())
                {
                    FCurrentUrl = frame.Url;
                    UpdateDom(frame);
                    UpdateDocumentSize();
                }
                FLoaded = true;
                // HACK: Re-apply zooming level :/ - https://vvvv.org/forum/htmltexture-bug-with-zoomlevel
                FBrowserHost.SetZoomLevel(ZoomLevel);
            }
            else
                // Reset computed values like document size or DOM
                Reset();
        }

        private readonly List<DoubleBufferedTexture> FTextures = new List<DoubleBufferedTexture>();

        internal void Paint(CefRectangle[] cefRects, IntPtr buffer, int stride, int width, int height)
        {
            // Do nothing if disabled
            if (!FEnabled) return;
            // If auto size is enabled ignore paint calls as long as document size is invalid
            if (IsAutoSize && !FDocumentSizeIsValid) return;
            lock (FTextures)
            {
                try
                {
                    for (int i = 0; i < cefRects.Length; i++)
                    {
                        var rect = new Rectangle(cefRects[i].X, cefRects[i].Y, cefRects[i].Width, cefRects[i].Height);
                        foreach (var texture in FTextures)
                        {
                            texture.Write(rect, buffer, stride);
                        }
                    }
                }
                catch (Exception e)
                {
                    FErrorText = e.ToString();
                }
            }
        }

        internal void UpdateResources(Device device)
        {
            lock (FTextures)
            {
                var size = Size;
                // Create new textures for valid sizes only
                if (size.Width > 0 && size.Height > 0)
                {
                    if (!FTextures.Any(t => t.Device == device))
                    {
                        var texture = new DoubleBufferedTexture(device, size);
                        FTextures.Add(texture);
                        // Trigger a redraw
                        FBrowserHost.Invalidate(CefPaintElementType.View);
                    }
                }
                // Tell all textures to update - in case the size is not valid
                // the texture will stick to the old one
                for (int i = 0; i < FTextures.Count; i++)
                {
                    var texture = FTextures[i];
                    if (texture.Device == device)
                    {
                        var newTexture = texture.Update(size);
                        // If the "new" texture is in a degraded state trigger a redraw
                        if (newTexture != texture && newTexture.IsDegraded)
                            FBrowserHost.Invalidate(CefPaintElementType.View);
                        FTextures[i] = newTexture;
                    }
                }
            }
        }

        internal EX9.Texture GetTexture(Device device)
        {
            lock (FTextures)
            {
                var texture = FTextures.FirstOrDefault(t => t.Device == device);
                if (texture != null)
                    return texture.LastCompleteTexture;
                else
                    return null;
            }
        }

        internal void DestroyResources(Device device)
        {
            lock (FTextures)
            {
                for (int i = FTextures.Count - 1; i >= 0; i--)
                {
                    var texture = FTextures[i];
                    if (texture.Device == device)
                    {
                        FTextures.Remove(texture);
                        texture.Dispose();
                    }
                }
            }
        }

        private void DestroyResources()
        {
            lock (FTextures)
            {
                foreach (var texture in FTextures)
                    texture.Dispose();
                FTextures.Clear();
            }
        }
    }
}
