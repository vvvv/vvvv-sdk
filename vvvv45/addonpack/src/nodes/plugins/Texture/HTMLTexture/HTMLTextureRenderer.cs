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

namespace VVVV.Nodes.Texture.HTML
{
    public class HTMLTextureRenderer : IDisposable
    {
        public const string DEFAULT_URL = "http://vvvv.org";
        public const int DEFAULT_WIDTH = 640;
        public const int DEFAULT_HEIGHT = 480;
        private readonly DXResource<EX9.Texture, CefBrowser> FTextureResource;
        private readonly List<EX9.Texture> FTextures = new List<EX9.Texture>();
        private readonly WebClient FWebClient;
        private volatile bool FEnabled;
        private CefBrowser FBrowser;
        private int FWidth = DEFAULT_WIDTH;
        private int FHeight = DEFAULT_HEIGHT;
        private string FUrl;
        private string FHtml = string.Empty;
        private double FZoomLevel;
        private MouseState FMouseState;
        private KeyboardState FKeyboardState = KeyboardState.Empty;
        private Vector2D FScrollTo;
        internal readonly object FLock = new object();
        internal XDocument FCurrentDom;
        internal int FFrameLoadCount;
        internal string FCurrentUrl;
        internal string FErrorText;
        internal ILogger Logger;

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

                FWebClient = new WebClient(this);
                CefBrowser.Create(windowInfo, FWebClient, DEFAULT_URL, settings);
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
            lock (FLock)
            {
                if (FBrowser != null)
                {
                    FBrowser.Close();
                    FBrowser.Dispose();
                }
                FTextureResource.Dispose();
            }
        }

        [Node(Name = "HTMLTexture", Category = "EX9.Texture", Version = "String")]
        public DXResource<EX9.Texture, CefBrowser> RenderString(
            out XDocument dom,
            out XElement rootElement,
            out bool isLoading,
            out string currentUrl,
            out string errorText,
            string baseUrl = DEFAULT_URL,
            string html = null,
            bool reload = false,
            int width = DEFAULT_WIDTH,
            int height = DEFAULT_HEIGHT,
            double zoomLevel = 0,
            MouseState mouseState = default(MouseState),
            KeyboardState keyboardState = default(KeyboardState),
            Vector2D scrollTo = default(Vector2D),
            bool updateDom = false,
            string javaScript = null,
            bool executeJavaScript = false,
            bool enabled = true
           )
        {
            if (!CheckState(out dom, out rootElement, out isLoading, out currentUrl, out errorText, enabled, updateDom))
            {
                return FTextureResource;
            }

            SetDimensions(width, height);

            if (FUrl != baseUrl || FHtml != html)
            {
                FUrl = baseUrl;
                FHtml = html ?? string.Empty;
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    byte[] utf8bytes = Encoding.Default.GetBytes(html);
                    html = Encoding.UTF8.GetString(utf8bytes);
                    mainFrame.LoadString(html, baseUrl);
                }
            }

            if (reload) FBrowser.Reload();

            SetZoomLevel(zoomLevel);
            SendEvents(mouseState, keyboardState);
            SetScroll(scrollTo);
            DoJavaScript(executeJavaScript, javaScript);

            return FTextureResource;
        }

        [Node(Name = "HTMLTexture", Category = "EX9.Texture", Version = "URL")]
        public DXResource<EX9.Texture, CefBrowser> RenderUrl(
            out XDocument dom,
            out XElement rootElement,
            out bool isLoading,
            out string currentUrl,
            out string errorText,
            string url = DEFAULT_URL,
            bool reload = false,
            int width = DEFAULT_WIDTH,
            int height = DEFAULT_HEIGHT,
            double zoomLevel = 0,
            MouseState mouseState = default(MouseState),
            KeyboardState keyboardState = default(KeyboardState),
            Vector2D scrollTo = default(Vector2D),
            bool updateDom = false,
            string javaScript = null,
            bool executeJavaScript = false,
            bool enabled = true
           )
        {
            if (!CheckState(out dom, out rootElement, out isLoading, out currentUrl, out errorText, enabled, updateDom))
            {
                return FTextureResource;
            }

            SetDimensions(width, height);

            if (FUrl != url)
            {
                FUrl = string.IsNullOrEmpty(url) ? "about:blank" : url;
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    mainFrame.LoadURL(url);
                }
            }

            if (reload) FBrowser.Reload();

            SetZoomLevel(zoomLevel);
            SendEvents(mouseState, keyboardState);
            SetScroll(scrollTo);
            DoJavaScript(executeJavaScript, javaScript);

            return FTextureResource;
        }

        private bool CheckState(
            out XDocument dom,
            out XElement rootElement,
            out bool isLoading,
            out string currentUrl,
            out string errorText,
            bool enabled,
            bool updateDom)
        {
            lock (FLock)
            {
                if (updateDom) UpdateDom();
                dom = FCurrentDom;
                rootElement = FCurrentDom != null ? FCurrentDom.Root : null;
                isLoading = IsLoading;
                currentUrl = FCurrentUrl;
                errorText = FErrorText;
            }

            Enabled = enabled;
            if (FBrowser == null || !enabled)
            {
                return false;
            }

            return true;
        }

        private void DoJavaScript(bool executeJavaScript, string javaScript)
        {
            if (executeJavaScript)
            {
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    mainFrame.ExecuteJavaScript(javaScript, string.Empty, 0);
                }
            }
        }

        private void SetDimensions(int width, int height)
        {
            // Normalize inputs
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            if (width != FWidth || height != FHeight)
            {
                FTextureResource.Dispose();
                FWidth = width;
                FHeight = height;
                FBrowser.SetSize(CefPaintElementType.View, width, height);
            }
        }

        private void SetZoomLevel(double zoomLevel)
        {
            if (FZoomLevel != zoomLevel)
            {
                FZoomLevel = zoomLevel;
                FBrowser.ZoomLevel = zoomLevel;
            }
        }

        private void SetScroll(Vector2D scrollTo)
        {
            if (FScrollTo != scrollTo)
            {
                FScrollTo = scrollTo;
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    var x = VMath.Map(scrollTo.x, 0, 1, 0, 1, TMapMode.Clamp);
                    var y = VMath.Map(scrollTo.y, 0, 1, 0, 1, TMapMode.Clamp);
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
            }
        }

        private void SendEvents(MouseState mouseState, KeyboardState keyboardState)
        {
            if (FMouseState != mouseState)
            {
                var x = (int)VMath.Map(mouseState.X, -1, 1, 0, FWidth, TMapMode.Clamp);
                var y = (int)VMath.Map(mouseState.Y, 1, -1, 0, FHeight, TMapMode.Clamp);
                var mouseDown = FMouseState.Button == MouseButton.None && mouseState.Button != MouseButton.None;
                var mouseUp = FMouseState.Button != MouseButton.None && mouseState.Button == MouseButton.None;
                var button = mouseState.Button;
                if (mouseUp) button = FMouseState.Button;
                if (mouseDown || mouseUp)
                {
                    switch (button)
                    {
                        case MouseButton.Left:
                            FBrowser.SendMouseClickEvent(x, y, CefMouseButtonType.Left, mouseUp, 1);
                            break;
                        case MouseButton.Middle:
                            FBrowser.SendMouseClickEvent(x, y, CefMouseButtonType.Middle, mouseUp, 1);
                            break;
                        case MouseButton.Right:
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
                var mouseWheelDelta = mouseState.MouseWheel - FMouseState.MouseWheel;
                if (mouseWheelDelta != 0)
                {
                    FBrowser.SendMouseWheelEvent(x, y, mouseWheelDelta, 0);
                }
                FMouseState = mouseState;
            }

            keyboardState = keyboardState ?? KeyboardState.Empty;
            if (FKeyboardState != keyboardState)
            {
                var isKeyUp = FKeyboardState.KeyCodes.Count > keyboardState.KeyCodes.Count;
                if (isKeyUp)
                {
                    var releasedKeys = FKeyboardState.KeyCodes.Except(keyboardState.KeyCodes);
                    var modifiers = (CefHandlerKeyEventModifiers)((int)(FKeyboardState.Modifiers) >> 16);
                    foreach (var key in releasedKeys)
                    {
                        var cefKey = new CefKeyInfo((int)key, false, false);
                        FBrowser.SendKeyEvent(CefKeyType.KeyUp, cefKey, modifiers);
                    }
                }
                var isKeyDown = keyboardState.KeyCodes.Count > FKeyboardState.KeyCodes.Count;
                if (isKeyDown)
                {
                    var pressedKeys = keyboardState.KeyCodes.Except(FKeyboardState.KeyCodes);
                    var modifiers = (CefHandlerKeyEventModifiers)((int)(keyboardState.Modifiers) >> 16);
                    foreach (var key in pressedKeys)
                    {
                        var cefKey = new CefKeyInfo((int)key, false, false);
                        FBrowser.SendKeyEvent(CefKeyType.KeyDown, cefKey, modifiers);
                    }
                }
                if (!isKeyUp)
                {
                    var keyChar = keyboardState.KeyChars.LastOrDefault();
                    if (keyChar != 0 && !(keyChar == '\t' || keyChar == '\b'))
                    {
                        var cefKey = new CefKeyInfo((int)keyChar, false, false);
                        var modifiers = (CefHandlerKeyEventModifiers)((int)(keyboardState.Modifiers) >> 16);
                        FBrowser.SendKeyEvent(CefKeyType.Char, cefKey, modifiers);
                    }
                }
                FKeyboardState = keyboardState;
            }
        }

        private void UpdateDom()
        {
            using (var mainFrame = FBrowser.GetMainFrame())
            {
                var domVisitor = new WebClient.DomVisitor(this);
                mainFrame.VisitDom(domVisitor);
            }
        }

        public bool IsLoading
        {
            get
            {
                lock (FLock)
                {
                    return FFrameLoadCount > 0;
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
                        if (FEnabled && FBrowser != null)
                        {
                            FBrowser.StopLoad();
                        }
                        FEnabled = value;
                        if (FEnabled && FBrowser != null)
                        {
                            FBrowser.Invalidate(new CefRect(0, 0, FWidth, FHeight));
                        }
                    }
                }
            }
        }

        internal void Attach(CefBrowser browser)
        {
            lock (FLock)
            {
                if (FBrowser != null)
                {
                    throw new InvalidOperationException("Browser already attached.");
                }

                FBrowser = browser;
                FBrowser.SetSize(CefPaintElementType.View, FWidth, FHeight);
            }
        }

        internal void Detach(CefBrowser browser)
        {
            lock (FLock)
            {
                if (FBrowser != null)
                {
                    FBrowser.Dispose();
                    FBrowser = null;
                }
            }
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

        internal void LoadURL(string url)
        {
            if (FBrowser != null)
            {
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    mainFrame.LoadURL(url);
                }
            }
        }

        private void WriteToTexture(Rectangle rect, IntPtr buffer, int stride, EX9.Texture texture)
        {
            // TODO: Do not lock entire surface.
            var dataRect = texture.LockRectangle(0, rect, LockFlags.None);
            try
            {
                var dataStream = dataRect.Data;
                if (rect.Width == FWidth && rect.Height == FHeight && dataRect.Pitch == stride)
                {
                    dataStream.WriteRange(buffer, FHeight * dataRect.Pitch);
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
                var texture = new EX9.Texture(device, FWidth, FHeight, 1, usage, Format.A8R8G8B8, pool);
                var rect = new CefRect(0, 0, FWidth, FHeight);
                if (FBrowser != null)
                {
                    FBrowser.Invalidate(rect);
                }
                FTextures.Add(texture);
                return texture;
            }
        }

        private void UpdateTexture(CefBrowser browser, EX9.Texture texture)
        {
            //            IntPtr buffer = Marshal.AllocHGlobal(FWidth * FHeight * 4);
            //            FBrowser.GetImage(CefPaintElementType.View, FWidth, FHeight, buffer);
            //            WriteToTexture(new Rectangle(0, 0, FWidth, FHeight), buffer, FWidth * 4, texture);
            //            Marshal.FreeHGlobal(buffer);
        }

        private void DestroyTexture(CefBrowser browser, EX9.Texture texture, DestroyReason reason)
        {
            lock (FTextures)
            {
                FTextures.Remove(texture);
                texture.Dispose();
            }
        }
    }
}
