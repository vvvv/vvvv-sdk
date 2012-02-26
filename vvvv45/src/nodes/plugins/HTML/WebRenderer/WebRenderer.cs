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
        private string FLastUrl;
        
        public WebRenderer()
        {
            CefService.AddRef();

            var settings = new CefBrowserSettings();
            settings.DeveloperToolsDisabled = true;
            settings.DragDropDisabled = true;
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
        public DXResource<Texture, CefBrowser> Render(string url, int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT)
        {
            if (FBrowser == null) return FTextureResource;
            
            if (width != FWidth || height != FHeight)
            {
                FWidth = width;
                FHeight = height;
                FBrowser.SetSize(CefPaintElementType.View, width, height);
                FTextureResource.Dispose();
            }
            if (FLastUrl != url)
            {
                FBrowser.GetMainFrame().LoadURL(url);
                FLastUrl = url;
            }
            return FTextureResource;
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
                var rect = new Rectangle(cefRect.X, cefRect.Y, cefRect.Width, cefRect.Height);
                foreach (var texture in FTextures)
                {
                    WriteToTexture(rect, buffer, stride, texture);
                }
            }
        }
        
        private void WriteToTexture(Rectangle rect, IntPtr buffer, int stride, Texture texture)
        {
            var dataRect = texture.LockRectangle(0, new Rectangle(0, 0, FWidth, FHeight), LockFlags.None);
            try
            {
                var dataStream = dataRect.Data;
                for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    dataStream.Position = y * dataRect.Pitch + 4 * rect.X;
                    dataStream.WriteRange(buffer + y * stride + 4 * rect.X, rect.Width * 4);
                }
            }
            finally
            {
                texture.UnlockRectangle(0);
            }
        }
        
        private Texture CreateTexture(CefBrowser browser, Device device)
        {
            lock (FTextures)
            {
                var buffer = Marshal.AllocHGlobal(FWidth * FHeight * 4);
                try
                {
                    var texture = new Texture(device, FWidth, FHeight, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
                    // This method always returns false?
                    if (FBrowser.GetImage(CefPaintElementType.View, FWidth, FHeight, buffer))
                    {
                        var rect = new Rectangle(0, 0, FWidth, FHeight);
                        WriteToTexture(rect, buffer, FWidth * 4, texture);
                    }
                    else
                    {
                        var rect = new CefRect(0, 0, FWidth, FHeight);
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
