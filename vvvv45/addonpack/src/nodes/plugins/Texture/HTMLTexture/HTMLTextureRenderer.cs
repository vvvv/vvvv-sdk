using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CefGlue;
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

namespace VVVV.Nodes.Texture.HTML
{
    public class HTMLTextureRenderer : IDisposable
    {
        public const string DEFAULT_URL = "http://vvvv.org";
        public const int DEFAULT_WIDTH = 640;
        public const int DEFAULT_HEIGHT = 480;
        private readonly DXResource<EX9.Texture, CefBrowser> FTextureResource;
        private readonly List<EX9.Texture> FTextures = new List<EX9.Texture>();
        private volatile bool FEnabled;
        private CefBrowser FBrowser;
        private string FUrl;
        private string FHtml = string.Empty;
        private readonly object FLock = new object();
        private XDocument FCurrentDom;
        private string FCurrentUrl;
        private string FErrorText;
        private ILogger Logger;
        private readonly AutoResetEvent FBrowserAttachedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent FBrowserDetachedEvent = new AutoResetEvent(false);

        public HTMLTextureRenderer(ILogger logger)
        {
            Logger = logger;

            var settings = new CefBrowserSettings();
            settings.DeveloperToolsDisabled = true;
            // TODO: Needs to be disabled or WebGL won't work at all.
            settings.AcceleratedCompositingEnabled = false;
            settings.FileAccessFromFileUrlsAllowed = true;
            settings.UniversalAccessFromFileUrlsAllowed = true;
            //settings.PluginsDisabled = true;

            using (var windowInfo = new CefWindowInfo())
            {
                windowInfo.TransparentPainting = true;
                windowInfo.SetAsOffScreen(IntPtr.Zero);

                var webClient = new WebClient(this);
                CefBrowser.Create(windowInfo, webClient, DEFAULT_URL, settings);
                // Block till the browser is attached
                FBrowserAttachedEvent.WaitOne();
            }

            FTextureResource = new DXResource<EX9.Texture, CefBrowser>(
                FBrowser,
                CreateTexture,
                UpdateTexture,
                DestroyTexture
               );
        }

        public void Dispose()
        {
            if (FBrowser != null)
            {
                FBrowser.Close();
                FBrowserDetachedEvent.WaitOne();
                lock (FLock)
                {
                    FBrowser.Dispose();
                    FBrowser = null;
                    FTextureResource.Dispose();
                }
                FBrowserAttachedEvent.Dispose();
                FBrowserDetachedEvent.Dispose();
            }
        }

        public void LoadURL(string url)
        {
            if (FUrl != url)
            {
                FUrl = string.IsNullOrEmpty(url) ? "about:blank" : url;
                DoLoadURL(FUrl);
            }
        }

        internal void DoLoadURL(string url)
        {
            using (var mainFrame = FBrowser.GetMainFrame())
            {
                mainFrame.LoadURL(url);
            }
        }

        public void LoadString(string html, string baseUrl)
        {
            if (FUrl != baseUrl || FHtml != html)
            {
                FUrl = baseUrl;
                FHtml = html ?? string.Empty;
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    mainFrame.LoadString(html, baseUrl);
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
            lock (FLock)
            {
                using (var frame = FBrowser.GetMainFrame())
                {
                    UpdateDom(frame);
                }
            }
        }

        private void UpdateDom(CefFrame frame)
        {
            var visitor = new DomVisitor(this);
            // Note: Call is async. Will at some point lead to OnVisitDom a few lines down.
            frame.VisitDom(visitor);
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
                    return FUrl;
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
                    lock (FTextures)
                    {
                        FTextureResource.Dispose();
                        FSize = size;
                        FBrowser.SetSize(CefPaintElementType.View, size.Width, size.Height);
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
                    FBrowser.ZoomLevel = value;
                    FZoomLevel = value;
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

        private MouseState FMouse;
        public MouseState Mouse
        {
            get { return FMouse; }
            set
            {
                if (FMouse != value)
                {
                    var x = (int)VMath.Map(value.X, -1, 1, 0, FSize.Width, TMapMode.Clamp);
                    var y = (int)VMath.Map(value.Y, 1, -1, 0, FSize.Height, TMapMode.Clamp);
                    var mouseDown = FMouse.Buttons == MouseButtons.None && value.Buttons != MouseButtons.None;
                    var mouseUp = FMouse.Buttons != MouseButtons.None && value.Buttons == MouseButtons.None;
                    var button = value.Buttons;
                    if (mouseUp) button = FMouse.Buttons;
                    if (mouseDown || mouseUp)
                    {
                        switch (button)
                        {
                            case MouseButtons.Left:
                                FBrowser.SendMouseClickEvent(x, y, CefMouseButtonType.Left, mouseUp, 1);
                                break;
                            case MouseButtons.Middle:
                                FBrowser.SendMouseClickEvent(x, y, CefMouseButtonType.Middle, mouseUp, 1);
                                break;
                            case MouseButtons.Right:
                                FBrowser.SendMouseClickEvent(x, y, CefMouseButtonType.Right, mouseUp, 1);
                                break;
                            default:
                                throw new Exception(string.Format("Unknown mouse button {0}.", button));
                        }
                    }
                    else
                    {
                        FBrowser.SendMouseMoveEvent(x, y, false);
                    }
                    var mouseWheelDelta = value.MouseWheel - FMouse.MouseWheel;
                    if (mouseWheelDelta != 0)
                    {
                        FBrowser.SendMouseWheelEvent(x, y, mouseWheelDelta, 0);
                    }
                    FMouse = value;
                }
            }
        }

        private KeyboardState FKeyboard = KeyboardState.Empty;
        public KeyboardState Keyboard
        {
            get { return FKeyboard; }
            set
            {
                value = value ?? KeyboardState.Empty;
                if (FKeyboard != value)
                {
                    var isKeyUp = FKeyboard.KeyCodes.Count > value.KeyCodes.Count;
                    if (isKeyUp)
                    {
                        var releasedKeys = FKeyboard.KeyCodes.Except(value.KeyCodes);
                        var modifiers = (CefHandlerKeyEventModifiers)((int)(FKeyboard.Modifiers) >> 16);
                        foreach (var key in releasedKeys)
                        {
                            var cefKey = new CefKeyInfo((int)key, false, false);
                            FBrowser.SendKeyEvent(CefKeyType.KeyUp, cefKey, modifiers);
                        }
                    }
                    var isKeyDown = value.KeyCodes.Count > FKeyboard.KeyCodes.Count;
                    if (isKeyDown)
                    {
                        var pressedKeys = value.KeyCodes.Except(FKeyboard.KeyCodes);
                        var modifiers = (CefHandlerKeyEventModifiers)((int)(value.Modifiers) >> 16);
                        foreach (var key in pressedKeys)
                        {
                            var cefKey = new CefKeyInfo((int)key, false, false);
                            FBrowser.SendKeyEvent(CefKeyType.KeyDown, cefKey, modifiers);
                        }
                    }
                    if (!isKeyUp)
                    {
                        var keyChar = value.KeyChars.LastOrDefault();
                        if (keyChar != 0 && !(keyChar == '\t' || keyChar == '\b'))
                        {
                            var cefKey = new CefKeyInfo((int)keyChar, false, false);
                            var modifiers = (CefHandlerKeyEventModifiers)((int)(value.Modifiers) >> 16);
                            FBrowser.SendKeyEvent(CefKeyType.Char, cefKey, modifiers);
                        }
                    }
                    FKeyboard = value;
                }
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
                        }
                        FEnabled = value;
                        if (FEnabled)
                        {
                            FBrowser.Invalidate(new CefRect(0, 0, FSize.Width, FSize.Height));
                        }
                    }
                }
            }
        }

        internal void Attach(CefBrowser browser)
        {
            FBrowser = browser;
            FBrowser.SetSize(CefPaintElementType.View, FSize.Width, FSize.Height);
            FBrowser.SendFocusEvent(true);
            FErrorText = string.Empty;
            FBrowserAttachedEvent.Set();
        }

        internal void Detach()
        {
            FBrowserDetachedEvent.Set();
        }

        internal void Paint(CefRect[] cefRects, IntPtr buffer, int stride)
        {
            if (!FEnabled) return;
            lock (FTextures)
            {
                try
                {
                    for (int i = 0; i < cefRects.Length; i++)
                    {
                        var rect = new Rectangle(cefRects[i].X, cefRects[i].Y, cefRects[i].Width, cefRects[i].Height);
                        foreach (var texture in FTextures)
                        {
                            WriteToTexture(rect, buffer, stride, texture);
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
            var dataRect = texture.LockRectangle(0, rect, LockFlags.None);
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
            // TODO: Fix exceptions on start up.
            lock (FTextures)
            {
                var usage = Usage.None & ~Usage.AutoGenerateMipMap;
                var pool = Pool.Managed;
                if (device is DeviceEx)
                {
                    usage = Usage.Dynamic & ~Usage.AutoGenerateMipMap;
                    pool = Pool.Default;
                }
                var texture = new EX9.Texture(device, FSize.Width, FSize.Height, 1, usage, Format.A8R8G8B8, pool);
                var rect = new CefRect(0, 0, FSize.Width, FSize.Height);
                FBrowser.Invalidate(rect);
                FTextures.Add(texture);
                return texture;
            }
        }

        private void UpdateTexture(CefBrowser browser, EX9.Texture texture)
        {
        }

        private void DestroyTexture(CefBrowser browser, EX9.Texture texture, DestroyReason reason)
        {
            lock (FTextures)
            {
                FTextures.Remove(texture);
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
                    FCurrentUrl = frame.GetURL();
                }
            }
        }

        internal void OnLoadError(CefFrame frame, CefHandlerErrorCode errorCode, string failedUrl, string errorText)
        {
            lock (FLock)
            {
                FLoadCount--;
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

        // Called on UI thread
        internal void OnVisitDom(CefDomDocument document)
        {
            try
            {
                using (var xmlReader = new CefXmlReader(document))
                {
                    var dom = XDocument.Load(xmlReader);
                    lock (FLock)
                    {
                        FCurrentDom = dom;
                    }
                }
            }
            catch (Exception e)
            {
                lock (FLock)
                {
                    FErrorText = e.ToString();
                }
            }
        }
    }
}
