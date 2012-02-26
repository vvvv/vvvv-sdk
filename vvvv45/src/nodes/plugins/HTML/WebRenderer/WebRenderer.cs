using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CefGlue;
using CefGlue.WebBrowser;
using CefGlue.Windows.Forms;
using SlimDX.Direct3D9;
using VVVV.Core;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.HTML
{
    public class WebRenderer : IDisposable
    {
        const string DEFAULT_URL = "http://vvvv.org";
        const int DEFAULT_WIDTH = 640;
        const int DEFAULT_HEIGHT = 480;
        private readonly DXResource<Texture, CefBrowser> FTextureResource;
        private readonly List<Texture> FTextures = new List<Texture>();
        private CefBrowser FBrowser;
        private int FWidth = DEFAULT_WIDTH;
        private int FHeight = DEFAULT_HEIGHT;
        private string FUrl;
        private double FZoomLevel;
        private MouseEvent FMouseEvent;
        internal int FFrameLoadCount;
        internal string FErrorText;
        
        public WebRenderer()
        {
            CefService.AddRef();

            var settings = new CefBrowserSettings();
            settings.DeveloperToolsDisabled = true;
            settings.DragDropDisabled = true;
            // TODO: Needs to be disabled or WebGL won't work at all.
            settings.AcceleratedCompositingEnabled = false;
            settings.FileAccessFromFileUrlsAllowed = true;
            settings.UniversalAccessFromFileUrlsAllowed = true;
            
            using (var windowInfo = new CefWindowInfo())
            {
                windowInfo.SetAsOffScreen(IntPtr.Zero);
                CefBrowser.Create(windowInfo, new WebClient(this), DEFAULT_URL, settings);
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
            FTextureResource.Dispose();
            FBrowser.Dispose();
            CefService.Release();
        }
        
        [Node(Name = "Renderer")]
        public DXResource<Texture, CefBrowser> Render(
            out bool isLoading,
            out string errorText,
            string url = DEFAULT_URL,
            int width = DEFAULT_WIDTH,
            int height = DEFAULT_HEIGHT,
            double zoomLevel = 0,
            MouseEvent mouseEvent = default(MouseEvent)
           )
        {
            if (FBrowser == null)
            {
                isLoading = IsLoading;
                errorText = "Initializing ...";
                return FTextureResource;
            }
            
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
                FBrowser.GetMainFrame().LoadURL(url);
            }
            if (FZoomLevel != zoomLevel)
            {
                FZoomLevel = zoomLevel;
                FBrowser.ZoomLevel = zoomLevel;
            }
            if (FMouseEvent != mouseEvent)
            {
                FMouseEvent = mouseEvent;
                var x = (int) VMath.Map(mouseEvent.X, -1, 1, 0, FWidth, TMapMode.Clamp);
                var y = (int) VMath.Map(mouseEvent.Y, 1, -1, 0, FHeight, TMapMode.Clamp);
                
                switch (mouseEvent.Button)
                {
                    case MouseButton.Left:
                        FBrowser.SendMouseClickEvent(x, y, CefMouseButtonType.Left, mouseEvent.MouseUp, mouseEvent.ClickCount);
                        break;
                    case MouseButton.Middle:
                        FBrowser.SendMouseClickEvent(x, y, CefMouseButtonType.Middle, mouseEvent.MouseUp, mouseEvent.ClickCount);
                        break;
                    case MouseButton.Right:
                        FBrowser.SendMouseClickEvent(x, y, CefMouseButtonType.Right, mouseEvent.MouseUp, mouseEvent.ClickCount);
                        break;
                    default:
                        if (mouseEvent.MouseUp)
                            FBrowser.SendMouseClickEvent(x, y, CefMouseButtonType.Left, mouseEvent.MouseUp, mouseEvent.ClickCount);
                        else
                            FBrowser.SendMouseMoveEvent(x, y, false);
                        break;
                }
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
        
        internal void Paint(CefRect cefRect, IntPtr buffer, int stride)
        {
            lock (FTextures)
            {
                try
                {
                    var rect = new Rectangle(cefRect.X, cefRect.Y, cefRect.Width, cefRect.Height);
                    foreach (var texture in FTextures)
                    {
                        WriteToTexture(rect, buffer, stride, texture);
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
            var dataRect = texture.LockRectangle(0, LockFlags.Discard);
            try
            {
                var dataStream = dataRect.Data;
                if (rect.Width == FWidth && rect.Height == FHeight && dataRect.Pitch == stride)
                {
                    dataStream.WriteRange(buffer, FHeight * dataRect.Pitch);
                }
                else
                {
                    for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                    {
                        dataStream.Position = y * dataRect.Pitch + 4 * rect.X;
                        dataStream.WriteRange(buffer + y * stride + 4 * rect.X, rect.Width * 4);
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
                var buffer = Marshal.AllocHGlobal(FWidth * FHeight * 4);
                try
                {
                    var texture = new Texture(device, FWidth, FHeight, 1, Usage.Dynamic & ~Usage.AutoGenerateMipMap, Format.A8R8G8B8, Pool.Default);
                    var rect = new CefRect(0, 0, FWidth, FHeight);
                    if (FBrowser != null)
                    {
                        FBrowser.Invalidate(rect);
                    }
                    FTextures.Add(texture);
                    return texture;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
        
        private void UpdateTexture(CefBrowser browser, Texture texture)
        {
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
