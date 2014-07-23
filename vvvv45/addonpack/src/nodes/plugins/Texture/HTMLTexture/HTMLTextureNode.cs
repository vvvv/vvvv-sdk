using System;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using Xilium.CefGlue;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using System.ComponentModel.Composition;
using VVVV.Utils.IO;
using System.Xml.Linq;
using EX9 = SlimDX.Direct3D9;
using System.Drawing;
using System.IO;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Texture.HTML
{
	[PluginInfo(Name = "HTMLTexture", 
                Category = "EX9.Texture", 
                Version = "String", 
                Tags = "browser, web, html, javascript, chrome, chromium, flash, webgl")]
    public class HTMLTextureStringNode : HTMLTextureNode
    {
        [Input("HTML", DefaultString = HTMLTextureRenderer.DEFAULT_CONTENT)]
        public ISpread<string> FHtmlIn;
        [Input("Base Url")]
        public ISpread<string> FBaseUrlIn;
        [Import]
        protected IPluginHost2 FHost;

        protected override void LoadContent(HTMLTextureRenderer renderer, Size size, int slice)
        {
            string patchPath;
            FHost.GetHostPath(out patchPath);
            renderer.LoadString(size, FHtmlIn[slice], FBaseUrlIn[slice], Path.GetDirectoryName(patchPath));
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

        protected override void LoadContent(HTMLTextureRenderer renderer, Size size, int slice)
        {
            renderer.LoadUrl(size, FUrlIn[slice]);
        }
    }

    public abstract class HTMLTextureNode : IPluginEvaluate, IDisposable, IPartImportsSatisfiedNotification, IPluginDXTexture2
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
        public IDXTextureOut FTextureOut;
        [Output("Root Element")]
        public ISpread<XElement> FRootElementOut;
        [Output("Document")]
        public ISpread<XDocument> FDomOut;
        [Output("Document Width")]
        public ISpread<int> FDocumentWidthOut;
        [Output("Document Height")]
        public ISpread<int> FDocumentHeightOut;
        [Output("Is Loading")]
        public ISpread<bool> FIsLoadingOut;
        [Output("Current Url")]
        public ISpread<string> FCurrentUrlOut;
        [Output("Error Text")]
        public ISpread<string> FErrorTextOut;

        [Import]
        protected ILogger FLogger;

        private readonly Spread<HTMLTextureRenderer> FWebRenderers = new Spread<HTMLTextureRenderer>();

        public virtual void OnImportsSatisfied()
        {
            FTextureOut.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            FWebRenderers.ResizeAndDispose(spreadMax, () => new HTMLTextureRenderer(FLogger));

            FTextureOut.SliceCount = spreadMax;
            FRootElementOut.SliceCount = spreadMax;
            FDomOut.SliceCount = spreadMax;
            FDocumentWidthOut.SliceCount = spreadMax;
            FDocumentHeightOut.SliceCount = spreadMax;
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
                LoadContent(webRenderer, new Size(FWidthIn[i], FHeightIn[i]), i);

                // Assign inputs
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
                if (FDomOut[i] != webRenderer.CurrentDom)
                    FDomOut[i] = webRenderer.CurrentDom;
                var rootElement = webRenderer.CurrentDom != null
                    ? webRenderer.CurrentDom.Root
                    : null;
                if (FRootElementOut[i] != rootElement)
                    FRootElementOut[i] = rootElement;
                var documentSize = webRenderer.DocumentSize;
                FDocumentWidthOut[i] = documentSize.Width;
                FDocumentHeightOut[i] = documentSize.Height;
                FIsLoadingOut[i] = webRenderer.IsLoading;
                FCurrentUrlOut[i] = webRenderer.CurrentUrl;
                FErrorTextOut[i] = webRenderer.CurrentError;
            }

            FTextureOut.MarkPinAsChanged();
        }

        protected abstract void LoadContent(HTMLTextureRenderer renderer, Size size, int slice);

        public void Dispose()
        {
            foreach (var renderer in FWebRenderers)
                renderer.Dispose();
        }

        EX9.Texture IPluginDXTexture2.GetTexture(IDXTextureOut pin, EX9.Device device, int slice)
        {
            var renderer = FWebRenderers[slice];
            return renderer.GetTexture(device);
        }

        void IPluginDXResource.UpdateResource(IPluginOut pin, EX9.Device device)
        {
            foreach (var renderer in FWebRenderers)
                renderer.UpdateResources(device);
        }

        void IPluginDXResource.DestroyResource(IPluginOut pin, EX9.Device device, bool onlyUnmanaged)
        {
            foreach (var renderer in FWebRenderers)
                renderer.DestroyResources(device);
        }
    }
}
