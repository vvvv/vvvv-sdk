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
using System.Drawing;

namespace VVVV.Nodes.Texture.HTML
{
	[PluginInfo(Name = "HTMLTexture", 
                Category = "EX9.Texture", 
                Version = "String", 
                Tags = "browser, web, html, javascript, chrome, chromium, flash, webgl")]
    public class HTMLTextureStringNode : HTMLTextureNode
    {
        [Input("HTML", DefaultString = @"<html><head></head><body bgcolor=""#ffffff""></body></html>")]
        public ISpread<string> FHtmlIn;
        [Input("Base Url", DefaultString = "about:blank")]
        public ISpread<string> FBaseUrlIn;

        protected override void LoadContent(HTMLTextureRenderer renderer, int slice)
        {
            renderer.LoadString(FHtmlIn[slice], FBaseUrlIn[slice]);
        }
    }

    [PluginInfo(Name = "HTMLTexture", 
                Category = "EX9.Texture", 
                Version = "URL", 
                Tags = "browser, web, html, javascript, chrome, chromium, flash, webgl")]
    public class HTMLTextureUrlNode : HTMLTextureNode
    {
        [Input("Url", DefaultString = HTMLTextureRenderer.DEFAULT_URL)]
        public ISpread<string> FUrlIn;

        protected override void LoadContent(HTMLTextureRenderer renderer, int slice)
        {
            renderer.LoadURL(FUrlIn[slice]);
        }
    }
    
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
        public ISpread<Mouse> FMouseIn;
        [Input("Key Event")]
        public ISpread<Keyboard> FKeyboardIn;
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
        public ISpread<DXResource<EX9.Texture, CefBrowser>> FTextureOut;
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
            FTextureOut.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            FWebRenderers.ResizeAndDispose(spreadMax, () => new HTMLTextureRenderer(FLogger));

            FTextureOut.SliceCount = spreadMax;
            FRootElementOut.SliceCount = spreadMax;
            FDomOut.SliceCount = spreadMax;
            FIsLoadingOut.SliceCount = spreadMax;
            FErrorTextOut.SliceCount = spreadMax;
            FCurrentUrlOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var webRenderer = FWebRenderers[i];

                // Check enabled state
                webRenderer.Enabled = FEnabledIn[i];
                if (!webRenderer.Enabled) continue;

                // LoadUrl or LoadString
                LoadContent(webRenderer, i);

                // Assign inputs
                webRenderer.Size = new Size(FWidthIn[i], FHeightIn[i]);
                webRenderer.ZoomLevel = FZoomLevelIn[i];
                webRenderer.Mouse = FMouseIn[i];
                webRenderer.Keyboard = FKeyboardIn[i];
                webRenderer.ScrollTo = FScrollToIn[i];

                if (FExecuteIn[i])
                    webRenderer.ExecuteJavaScript(FJavaScriptIn[i]);

                if (FUpdateDomIn[i])
                    webRenderer.UpdateDom();

                if (FReloadIn[i])
                    webRenderer.Reload();

                // Set outputs
                FTextureOut[i] = webRenderer.TextureResource;
                if (FDomOut[i] != webRenderer.CurrentDom)
                    FDomOut[i] = webRenderer.CurrentDom;
                var rootElement = webRenderer.CurrentDom != null
                    ? webRenderer.CurrentDom.Root
                    : null;
                if (FRootElementOut[i] != rootElement)
                    FRootElementOut[i] = rootElement;
                FIsLoadingOut[i] = webRenderer.IsLoading;
                FCurrentUrlOut[i] = webRenderer.CurrentUrl;
                FErrorTextOut[i] = webRenderer.CurrentError;
            }
        }

        protected abstract void LoadContent(HTMLTextureRenderer renderer, int slice);

        public void Dispose()
        {
            FWebRenderers.ResizeAndDispose(0, () => new HTMLTextureRenderer(FLogger));
        }
    }
}
