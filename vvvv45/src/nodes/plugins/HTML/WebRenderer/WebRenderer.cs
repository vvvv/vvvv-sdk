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
        //        private Bitmap FBitmap;
        private int FWidth = DEFAULT_WIDTH;
        private int FHeight = DEFAULT_HEIGHT;
        private string FLastUrl;
        
        public WebRenderer()
        {
            // TODO: Move this to a more central location
            var cefSettings = new CefSettings();
            cefSettings.MultiThreadedMessageLoop = true;
            cefSettings.CachePath = Path.GetDirectoryName(Application.ExecutablePath) + "/cache";
            cefSettings.LogFile = Path.GetDirectoryName(Application.ExecutablePath) + "/CEF.log";
            cefSettings.LogSeverity = CefLogSeverity.Verbose;
            Cef.Initialize(cefSettings);

            var settings = new CefBrowserSettings();
            //            settings.JavaDisabled = true;
            //            settings.JavaScriptDisabled = true;
            //            settings.WebGLDisabled = true;
            
            using (var windowInfo = new CefWindowInfo())
            {
                //windowInfo.SetAsOffScreen(IntPtr.Zero);
                //                var control = new Form();
                //                control.Show();
                windowInfo.SetAsOffScreen(IntPtr.Zero);
                CefBrowser.Create(windowInfo, new WebClient(this), DEFAULT_URL, settings);
                //                var browser = new CefWebBrowser();
                //                browser.LoadUrl(DEFAULT_URL);
                //                var browser = new CefWebBrowserCore(this, settings, DEFAULT_URL);
                //                browser.Create(windowInfo);
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
            
            // TODO: Move this to a more central location
            Cef.Shutdown();
        }
        
        [Node]
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
                FBrowser.GetMainFrame().
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
                foreach (var texture in FTextures)
                {
                    try {
                        var rect = new Rectangle(cefRect.X, cefRect.Y, cefRect.Width, cefRect.Height);
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
                    } catch (Exception e) {
                        throw;
                    }
                }
            }
        }
        
        private Texture CreateTexture(CefBrowser browser, Device device)
        {
            lock (FTextures)
            {
                var texture = new Texture(device, FWidth, FHeight, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
                FTextures.Add(texture);
                return texture;
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
