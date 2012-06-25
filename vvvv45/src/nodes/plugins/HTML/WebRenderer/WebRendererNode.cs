using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using SlimDX.Direct3D9;
using CefGlue;

namespace VVVV.Nodes.HTML
{
    [PluginInfo(Name = "Renderer", Category = "HTML", Version = "Chrome", Tags = "browser, web, html, javascript, renderer, chrome, flash, webgl, texture")]
    public class WebRendererNode : IPluginEvaluate, IDisposable
    {
        [Input("Url", DefaultString = WebRenderer.DEFAULT_URL)]
        public ISpread<string> FUrlIn;
        [Input("HTML", DefaultString = "")]
        public ISpread<string> FHtmlIn;
        [Input("Reload", IsBang = true)]
        public ISpread<bool> FReloadIn;
        [Input("Width", DefaultValue = WebRenderer.DEFAULT_WIDTH)]
        public ISpread<int> FWidthIn;
        [Input("Height", DefaultValue = WebRenderer.DEFAULT_HEIGHT)]
        public ISpread<int> FHeightIn;
        [Input("Zoom Level")]
        public ISpread<double> FZoomLevelIn;
        [Input("Mouse Event")]
        public ISpread<MouseState> FMouseEventIn;
        [Input("Key Event")]
        public ISpread<KeyState> FKeyEventIn;
        [Input("Enabled", DefaultValue = 1)]
        public ISpread<bool> FEnabledIn;

        [Output("Output")]
        public ISpread<DXResource<Texture, CefBrowser>> FOutput;
        [Output("Is Loading")]
        public ISpread<bool> FIsLoadingOut;
        [Output("Current Url")]
        public ISpread<string> FCurrentUrlOut;
        [Output("Error Text")]
        public ISpread<string> FErrorTextOut;

        private readonly Spread<WebRenderer> FWebRenderers = new Spread<WebRenderer>();

        public void Evaluate(int spreadMax)
        {
            FWebRenderers.ResizeAndDispose(spreadMax, () => new WebRenderer());

            FOutput.SliceCount = spreadMax;
            FIsLoadingOut.SliceCount = spreadMax;
            FErrorTextOut.SliceCount = spreadMax;
            FCurrentUrlOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var webRenderer = FWebRenderers[i];
                var url = FUrlIn[i];
                var html = FHtmlIn[i];
                var reload = FReloadIn[i];
                var width = FWidthIn[i];
                var height = FHeightIn[i];
                var zoomLevel = FZoomLevelIn[i];
                var mouseEvent = FMouseEventIn[i];
                var keyEvent = FKeyEventIn[i];
                var enabled = FEnabledIn[i];
                bool isLoading;
                string currentUrl, errorText;
                var output = webRenderer.Render(out isLoading, out currentUrl, out errorText, url, html, reload, width, height, zoomLevel, mouseEvent, keyEvent, enabled);
                FOutput[i] = output;
                FIsLoadingOut[i] = isLoading;
                FCurrentUrlOut[i] = currentUrl;
                FErrorTextOut[i] = errorText;
            }
        }

        public void Dispose()
        {
            FWebRenderers.ResizeAndDispose(0);
        }
    }
}
