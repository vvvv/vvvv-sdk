using System;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using CefGlue;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using System.ComponentModel.Composition;
using VVVV.Utils.IO;
using System.Xml.Linq;
using EX9 = SlimDX.Direct3D9;

namespace VVVV.Nodes.Texture.HTML
{
    public abstract class HTMLTextureNode : IPluginEvaluate, IDisposable, IPartImportsSatisfiedNotification
    {
        [Input("Reload", IsBang = true)]
        public ISpread<bool> FReloadIn;
        [Input("Width", DefaultValue = HTMLTextureRenderer.DEFAULT_WIDTH)]
        public ISpread<int> FWidthIn;
        [Input("Height", DefaultValue = HTMLTextureRenderer.DEFAULT_HEIGHT)]
        public ISpread<int> FHeightIn;
        [Input("Zoom Level")]
        public ISpread<double> FZoomLevelIn;
        [Input("Mouse Event")]
        public ISpread<MouseState> FMouseEventIn;
        [Input("Key Event")]
        public ISpread<KeyboardState> FKeyEventIn;
        [Input("Scroll To")]
        public ISpread<Vector2D> FScrollToIn;
        [Input("Update DOM", IsBang = true)]
        public ISpread<bool> FUpdateDomIn;
        [Input("JavaScript")]
        public ISpread<string> FJavaScriptIn;
        [Input("Execute", IsBang = true)]
        public ISpread<bool> FExecuteIn;
        [Input("Enabled", DefaultValue = 1)]
        public ISpread<bool> FEnabledIn;

        [Output("Output")]
        public ISpread<DXResource<EX9.Texture, CefBrowser>> FOutput;
        [Output("Root Element")]
        public ISpread<XElement> FRootElementOut;
        [Output("Document")]
        public ISpread<XDocument> FDomOut;
        [Output("Is Loading")]
        public ISpread<bool> FIsLoadingOut;
        [Output("Current Url")]
        public ISpread<string> FCurrentUrlOut;
        [Output("Error Text")]
        public ISpread<string> FErrorTextOut;

        [Import]
        private ILogger FLogger;

        private readonly Spread<HTMLTextureRenderer> FWebRenderers = new Spread<HTMLTextureRenderer>();

        public void OnImportsSatisfied()
        {
            FOutput.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            FWebRenderers.ResizeAndDispose(spreadMax, () => new HTMLTextureRenderer(FLogger));

            FOutput.SliceCount = spreadMax;
            FRootElementOut.SliceCount = spreadMax;
            FDomOut.SliceCount = spreadMax;
            FIsLoadingOut.SliceCount = spreadMax;
            FErrorTextOut.SliceCount = spreadMax;
            FCurrentUrlOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var webRenderer = FWebRenderers[i];
                var reload = FReloadIn[i];
                var width = FWidthIn[i];
                var height = FHeightIn[i];
                var zoomLevel = FZoomLevelIn[i];
                var mouseEvent = FMouseEventIn[i];
                var keyEvent = FKeyEventIn[i];
                var scrollTo = FScrollToIn[i];
                var updateDom = FUpdateDomIn[i];
                var javaScript = FJavaScriptIn[i];
                var execute = FExecuteIn[i];
                var enabled = FEnabledIn[i];
                XDocument dom; 
                XElement rootElement;
                bool isLoading;
                string currentUrl, errorText;
                var output = DoRenderCall(
                    webRenderer,
                    i,
                    out dom,
                    out rootElement,
                    out isLoading, 
                    out currentUrl, 
                    out errorText,
                    reload,
                    width, 
                    height, 
                    zoomLevel,
                    mouseEvent,
                    keyEvent,
                    scrollTo,
                    updateDom,
                    javaScript,
                    execute,
                    enabled);
                if (FOutput[i] != output) FOutput[i] = output;
                if (FDomOut[i] != dom) FDomOut[i] = dom;
                if (FRootElementOut[i] != rootElement) FRootElementOut[i] = rootElement;
                FIsLoadingOut[i] = isLoading;
                if (FCurrentUrlOut[i] != currentUrl) FCurrentUrlOut[i] = currentUrl;
                if (FErrorTextOut[i] != errorText) FErrorTextOut[i] = errorText;
            }
        }

        protected abstract DXResource<EX9.Texture, CefBrowser> DoRenderCall(
            HTMLTextureRenderer webRenderer,
            int slice,
            out XDocument dom, 
            out XElement rootElement, 
            out bool isLoading, 
            out string currentUrl, 
            out string errorText, 
            bool reload, 
            int width, 
            int height, 
            double zoomLevel, 
            MouseState mouseEvent, 
            KeyboardState keyEvent, 
            Vector2D scrollTo, 
            bool updateDom,
            string javaScript, 
            bool execute, 
            bool enabled);

        public void Dispose()
        {
            FWebRenderers.ResizeAndDispose(0, () => new HTMLTextureRenderer(FLogger));
        }
    }

    [PluginInfo(Name = "HTMLTexture", Category = "EX9.Texture", Version = "String", Tags = "browser, web, html, javascript, chromium, flash, webgl")]
    public class HTMLTextureStringNode : HTMLTextureNode
    {
        [Input("HTML", DefaultString = @"<html><head></head><body bgcolor=""#ffffff""></body></html>")]
        public ISpread<string> FHtmlIn;
        [Input("Base Url", DefaultString = "about:blank")]
        public ISpread<string> FBaseUrlIn;

        protected override DXResource<EX9.Texture, CefBrowser> DoRenderCall(HTMLTextureRenderer webRenderer, int slice, out XDocument dom, out XElement rootElement, out bool isLoading, out string currentUrl, out string errorText, bool reload, int width, int height, double zoomLevel, MouseState mouseEvent, KeyboardState keyEvent, Vector2D scrollTo, bool updateDom, string javaScript, bool execute, bool enabled)
        {
            return webRenderer.RenderString(
                    out dom,
                    out rootElement,
                    out isLoading,
                    out currentUrl,
                    out errorText,
                    FBaseUrlIn[slice],
                    FHtmlIn[slice],
                    reload,
                    width,
                    height,
                    zoomLevel,
                    mouseEvent,
                    keyEvent,
                    scrollTo,
                    updateDom,
                    javaScript,
                    execute,
                    enabled);
        }
    }

    [PluginInfo(Name = "HTMLTexture", Category = "EX9.Texture", Version = "URL", Tags = "browser, web, html, javascript, chromium, flash, webgl")]
    public class HTMLTextureUrlNode : HTMLTextureNode
    {
        [Input("Url", DefaultString = HTMLTextureRenderer.DEFAULT_URL)]
        public ISpread<string> FUrlIn;

        protected override DXResource<EX9.Texture, CefBrowser> DoRenderCall(HTMLTextureRenderer webRenderer, int slice, out XDocument dom, out XElement rootElement, out bool isLoading, out string currentUrl, out string errorText, bool reload, int width, int height, double zoomLevel, MouseState mouseEvent, KeyboardState keyEvent, Vector2D scrollTo, bool updateDom, string javaScript, bool execute, bool enabled)
        {
            return webRenderer.RenderUrl(
                    out dom,
                    out rootElement,
                    out isLoading,
                    out currentUrl,
                    out errorText,
                    FUrlIn[slice],
                    reload,
                    width,
                    height,
                    zoomLevel,
                    mouseEvent,
                    keyEvent,
                    scrollTo,
                    updateDom,
                    javaScript,
                    execute,
                    enabled);
        }
    }
}
