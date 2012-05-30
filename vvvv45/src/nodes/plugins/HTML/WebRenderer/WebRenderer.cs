using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CefGlue;
using CefGlue.WebBrowser;
using SlimDX.Direct3D9;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VMath;
using System.Threading;

namespace VVVV.Nodes.HTML
{
    public class WebRenderer : IDisposable
    {
        public const string DEFAULT_URL = "http://vvvv.org";
        public const int DEFAULT_WIDTH = 640;
        public const int DEFAULT_HEIGHT = 480;
        private readonly DXResource<Texture, CefBrowser> FTextureResource;
        private readonly List<Texture> FTextures = new List<Texture>();
        private readonly WebClient FWebClient;
        private volatile bool FEnabled;
        private CefBrowser FBrowser;
        private int FWidth = DEFAULT_WIDTH;
        private int FHeight = DEFAULT_HEIGHT;
        private string FUrl;
        private double FZoomLevel;
        private MouseState FMouseState;
        private KeyState FKeyState;
        private Form FForm;
        internal int FFrameLoadCount;
        internal string FErrorText;

        public WebRenderer()
        {
            CefService.AddRef();

            var settings = new CefBrowserSettings();
            settings.DeveloperToolsDisabled = true;
            // TODO: Needs to be disabled or WebGL won't work at all.
            settings.AcceleratedCompositingEnabled = false;
            settings.FileAccessFromFileUrlsAllowed = true;
            settings.UniversalAccessFromFileUrlsAllowed = true;
            //settings.PluginsDisabled = true;

            FForm = new Form();
            using (var windowInfo = new CefWindowInfo())
            {
                windowInfo.TransparentPainting = true;
                windowInfo.SetAsOffScreen(FForm.Handle);
                //windowInfo.SetAsOffScreen(IntPtr.Zero);

                FWebClient = new WebClient(this);
                CefBrowser.Create(windowInfo, FWebClient, DEFAULT_URL, settings);
            }

            FTextureResource = new DXResource<Texture, CefBrowser>(
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
            }
            FTextureResource.Dispose();
            CefService.Release();
        }

        [Node(Name = "Renderer")]
        public DXResource<Texture, CefBrowser> Render(
            out bool isLoading,
            out string errorText,
            string url = DEFAULT_URL,
            bool reload = false,
            int width = DEFAULT_WIDTH,
            int height = DEFAULT_HEIGHT,
            double zoomLevel = 0,
            MouseState mouseState = default(MouseState),
            KeyState keyState = default(KeyState),
            bool enabled = true
           )
        {
            if (FBrowser == null)
            {
                isLoading = IsLoading;
                errorText = "Initializing ...";
                return FTextureResource;
            }

            Enabled = enabled;
            if (!Enabled)
            {
                isLoading = false;
                errorText = "Disabled";
                return FTextureResource;
            }

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

            if (FUrl != url)
            {
                FUrl = url;
                using (var mainFrame = FBrowser.GetMainFrame())
                {
                    mainFrame.LoadURL(url);
                }
            }
            if (reload) FBrowser.Reload();
            if (FZoomLevel != zoomLevel)
            {
                FZoomLevel = zoomLevel;
                FBrowser.ZoomLevel = zoomLevel;
            }
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
                if (mouseState.MouseWheelDelta != 0)
                {
                    FBrowser.SendMouseWheelEvent(x, y, mouseState.MouseWheelDelta);
                }
                FMouseState = mouseState;
            }

            if (FKeyState != keyState)
            {
                var isKeyUp = FKeyState.KeyCode != Keys.None;
                if (isKeyUp)
                {
                    var modifiers = (CefHandlerKeyEventModifiers)((int)(FKeyState.KeyCode & Keys.Modifiers) >> 16);
                    var key = (int)(FKeyState.KeyCode & ~Keys.Modifiers);
                    FBrowser.SendKeyEvent(CefKeyType.KeyUp, key, modifiers, false, false);
                }
                var isKeyDown = keyState.KeyCode != Keys.None;
                if (isKeyDown)
                {
                    var modifiers = (CefHandlerKeyEventModifiers)((int)(keyState.KeyCode & Keys.Modifiers) >> 16);
                    var key = (int)(keyState.KeyCode & ~Keys.Modifiers);
                    FBrowser.SendKeyEvent(CefKeyType.KeyDown, key, modifiers, false, false);
                }
                var isKeyPress = keyState.Key.HasValue && !(keyState.Key == '\t' || keyState.Key == '\b');
                if (isKeyPress)
                {
                    var modifiers = (CefHandlerKeyEventModifiers)((int)(keyState.KeyCode & Keys.Modifiers) >> 16);
                    var key = (int)keyState.Key;
                    FBrowser.SendKeyEvent(CefKeyType.Char, key, modifiers, false, false);
                }
                FKeyState = keyState;
            }

            isLoading = IsLoading;
            errorText = FErrorText;

            return FTextureResource;
        }

        public bool IsLoading
        {
            get
            {
                return FFrameLoadCount > 0;
            }
        }

        public bool Enabled
        {
            get { return FEnabled; }
            set
            {
                if (FEnabled != value)
                {
                    FEnabled = value;
                    if (FEnabled)
                    {
                        FBrowser.Invalidate(new CefRect(0, 0, FWidth, FHeight));
                    }
                }
            }
        }

        internal void Attach(CefBrowser browser)
        {
            if (FBrowser != null)
            {
                throw new InvalidOperationException("Browser already attached.");
            }

            FBrowser = browser;
            FBrowser.SetSize(CefPaintElementType.View, FWidth, FHeight);
        }

        internal void Detach()
        {
            if (FBrowser != null)
            {
                FBrowser.Dispose();
                FBrowser = null;
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

        private void WriteToTexture(Rectangle rect, IntPtr buffer, int stride, Texture texture)
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

        private Texture CreateTexture(CefBrowser browser, Device device)
        {
            // TODO: Fix exceptions on start up.
            lock (FTextures)
            {
                var texture = new Texture(device, FWidth, FHeight, 1, Usage.None & ~Usage.AutoGenerateMipMap, Format.A8R8G8B8, Pool.Managed);
                var rect = new CefRect(0, 0, FWidth, FHeight);
                if (FBrowser != null)
                {
                    FBrowser.Invalidate(rect);
                }
                FTextures.Add(texture);
                return texture;
            }
        }

        private void UpdateTexture(CefBrowser browser, Texture texture)
        {
            //            IntPtr buffer = Marshal.AllocHGlobal(FWidth * FHeight * 4);
            //            FBrowser.GetImage(CefPaintElementType.View, FWidth, FHeight, buffer);
            //            WriteToTexture(new Rectangle(0, 0, FWidth, FHeight), buffer, FWidth * 4, texture);
            //            Marshal.FreeHGlobal(buffer);
        }

        private void DestroyTexture(CefBrowser browser, Texture texture)
        {
            lock (FTextures)
            {
                FTextures.Remove(texture);
                texture.Dispose();
            }
        }
    }
}
