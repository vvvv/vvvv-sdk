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

namespace VVVV.Nodes.Texture.HTML
{
    public class HTMLTextureRenderer : IDisposable
    {
        public const string DEFAULT_URL = "http://vvvv.org";
        public const int DEFAULT_WIDTH = 640;
        public const int DEFAULT_HEIGHT = 480;
        private readonly DXResource<EX9.Texture, CefBrowser> FTextureResource;
        private volatile bool FEnabled;
        private readonly WebClient FWebClient;
        private CefBrowser FBrowser;
        private CefBrowserHost FBrowserHost;
        private string FUrl;
        private string FHtml;
        private readonly object FLock = new object();
        private XDocument FCurrentDom;
        private string FCurrentUrl;
        private string FErrorText;
        public ILogger Logger;
        private readonly AutoResetEvent FBrowserAttachedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent FBrowserDetachedEvent = new AutoResetEvent(false);

        public HTMLTextureRenderer(ILogger logger)
        {
            Logger = logger;

            var settings = new CefBrowserSettings();
            settings.AcceleratedCompositing = CefState.Enabled;
            settings.FileAccessFromFileUrls = CefState.Enabled;
            settings.UniversalAccessFromFileUrls = CefState.Enabled;

            var windowInfo = CefWindowInfo.Create();
            windowInfo.TransparentPainting = true;
            windowInfo.SetAsOffScreen(IntPtr.Zero);

            FWebClient = new WebClient(this);
            // See http://magpcss.org/ceforum/viewtopic.php?f=6&t=5901
            CefBrowserHost.CreateBrowser(windowInfo, FWebClient, settings);
            // Block until browser is created
            FBrowserAttachedEvent.WaitOne();

            FTextureResource = new DXResource<EX9.Texture, CefBrowser>(
                FBrowser,
                CreateTexture,
                UpdateTexture,
                DestroyTexture
               );
        }

        internal void Attach(CefBrowser browser)
        {
            FBrowser = browser;
            FBrowserHost = browser.GetHost();
            FBrowserHost.SetMouseCursorChangeDisabled(true);
            FBrowserAttachedEvent.Set();
        }

        internal void Detach()
        {
            FBrowserDetachedEvent.Set();
        }

        public void Dispose()
        {
            FBrowserHost.CloseBrowser(true);
            FBrowserDetachedEvent.WaitOne();
            lock (FLock)
            {
                FBrowser.Dispose();
                FTextureResource.Dispose();
            }
            FBrowserAttachedEvent.Dispose();
            FBrowserDetachedEvent.Dispose();
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
        }

        public void LoadUrl(string url)
        {
            if (FUrl != url)
            {
                FUrl = string.IsNullOrEmpty(url) ? "about:blank" : url;
                FHtml = null;
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    // Seems important that url is valid
                    var uri = new UriBuilder(FUrl).Uri;
                    mainFrame.LoadUrl(uri.ToString());
                }
            }
        }

        public void LoadString(string html, string baseUrl)
        {
            if (FUrl != baseUrl || FHtml != html)
            {
                FUrl = string.IsNullOrEmpty(baseUrl) ? "about:blank" : baseUrl;
                FHtml = html ?? string.Empty;
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    // Seems important that url is valid
                    var uri = new UriBuilder(FUrl).Uri;
                    mainFrame.LoadUrl(uri.ToString());
                }
            }
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

        public void Reload()
        {
            FBrowser.Reload();
        }

        public DXResource<EX9.Texture, CefBrowser> TextureResource
        {
            get { return FTextureResource; }
        }

        public XDocument CurrentDom
        {
            get 
            {
                lock (FLock)
                {
                    return FCurrentDom;
                }
            }
        }

        public string CurrentUrl
        {
            get 
            {
                lock (FLock)
                {
                    return FCurrentUrl;
                }
            }
        }

        public string CurrentError
        {
            get 
            {
                lock (FLock)
                {
                    return FErrorText;
                }
            }
        }

        public string CurrentHTML
        {
            get { return FHtml; }
        }

        private Size FSize = new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT);
        public Size Size
        {
            get { return FSize; }
            set
            {
                // Normalize inputs
                var size = new Size(Math.Max(1, value.Width), Math.Max(1, value.Height));
                if (size != FSize)
                {
                    lock (FTextureResource)
                    {
                        FTextureResource.Dispose();
                        FSize = size;
                        FBrowserHost.WasResized();
                    }
                }
            }
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
                                (int)VMath.Map(n.Position.X, 0, n.ClientArea.Width, 0, FSize.Width, TMapMode.Clamp),
                                (int)VMath.Map(n.Position.Y, 0, n.ClientArea.Height, 0, FSize.Height, TMapMode.Clamp),
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

        private int FLoadCount;
        public bool IsLoading
        {
            get
            {
                lock (FLock)
                {
                    return FLoadCount > 0;
                }
            }
        }

        public bool Enabled
        {
            get { return FEnabled; }
            set
            {
                if (FEnabled != value)
                {
                    lock (FLock)
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
                            FBrowserHost.Invalidate(new CefRectangle(0, 0, FSize.Width, FSize.Height), CefPaintElementType.View);
                        }
                    }
                }
            }
        }

        internal void Paint(CefRectangle[] cefRects, IntPtr buffer, int stride)
        {
            if (!FEnabled) return;
            lock (FTextureResource)
            {
                try
                {
                    for (int i = 0; i < cefRects.Length; i++)
                    {
                        var rect = new Rectangle(cefRects[i].X, cefRects[i].Y, cefRects[i].Width, cefRects[i].Height);
                        foreach (var texture in FTextureResource.DeviceResources)
                        {
                            var sysmemTexture = texture.Tag as EX9.Texture;
                            WriteToTexture(rect, buffer, stride, sysmemTexture);
                        }
                    }
                }
                catch (Exception e)
                {
                    FErrorText = e.ToString();
                }
            }
        }

        private void WriteToTexture(Rectangle rect, IntPtr buffer, int stride, EX9.Texture texture)
        {
            // Rect needs to be inside of Width/Height
            rect = Rectangle.Intersect(new Rectangle(0, 0, FSize.Width, FSize.Height), rect);
            if (rect == Rectangle.Empty) return;
            var dataRect = texture.LockRectangle(0, rect, LockFlags.Discard);
            try
            {
                var dataStream = dataRect.Data;
                if (rect.Width == FSize.Width && rect.Height == FSize.Height && dataRect.Pitch == stride)
                {
                    dataStream.WriteRange(buffer, FSize.Height * dataRect.Pitch);
                }
                else
                {
                    var offset = stride * rect.Y + 4 * rect.X;
                    var source = buffer + offset;
                    for (int y = 0; y < rect.Height; y++)
                    {
                        dataStream.Position = y * dataRect.Pitch;
                        dataStream.WriteRange(source + y * stride, rect.Width * 4);
                    }
                }
            }
            finally
            {
                texture.UnlockRectangle(0);
            }
        }

        private EX9.Texture CreateTexture(CefBrowser browser, Device device)
        {
            lock (FTextureResource)
            {
                var usage = Usage.None & ~Usage.AutoGenerateMipMap;
                var texture = new EX9.Texture(device, FSize.Width, FSize.Height, 1, usage, Format.A8R8G8B8, Pool.Default);
                var sysmemTexture = new EX9.Texture(device, FSize.Width, FSize.Height, 1, usage, Format.A8R8G8B8, Pool.SystemMemory);
                texture.Tag = sysmemTexture;
                var rect = new CefRectangle(0, 0, FSize.Width, FSize.Height);
                FBrowserHost.Invalidate(rect, CefPaintElementType.View);
                return texture;
            }
        }

        private void UpdateTexture(CefBrowser browser, EX9.Texture texture)
        {
            lock (FTextureResource)
            {
                var sysmemTexture = texture.Tag as EX9.Texture;
                texture.Device.UpdateTexture(sysmemTexture, texture);
            }
        }

        private void DestroyTexture(CefBrowser browser, EX9.Texture texture, DestroyReason reason)
        {
            lock (FTextureResource)
            {
                var sysmemTexture = texture.Tag as EX9.Texture;
                sysmemTexture.Dispose();
                texture.Dispose();
            }
        }

        internal void OnLoadStart(CefFrame frame)
        {
            lock (FLock)
            {
                FLoadCount++;
                if (frame.IsMain)    
                {
                    
                    FCurrentDom = null;
                    FErrorText = string.Empty;
                    FCurrentUrl = frame.Url;
                }
            }
        }

        internal void OnLoadError(CefFrame frame, CefErrorCode errorCode, string failedUrl, string errorText)
        {
            lock (FLock)
            {
                if (frame.IsMain)
                {
                    FCurrentDom = null;
                    FErrorText = errorText;
                }
            }
        }

        internal void OnLoadEnd(CefFrame frame, int httpStatusCode)
        {
            lock (FLock)
            {
                FLoadCount--;
                if (frame.IsMain)
                {
                    UpdateDom(frame);
                }
            }
        }

        internal void OnUpdateDom(XDocument dom)
        {
            lock (FLock)
            {
                FCurrentDom = dom;
            }
        }

        internal void OnError(string error)
        {
            lock (FLock)
                FErrorText = error;
        }
    }
}
