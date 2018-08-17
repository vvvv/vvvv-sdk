using System;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using System.Drawing;
using System.IO;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;
using VL.Lib.Basics.Imaging;

namespace VVVV.Nodes.Texture.HTML
{
    [PluginInfo(Name = "HTMLView", 
                Category = "Image", 
                Version = "String", 
                Credits = "Development sponsored by http://meso.net",
                Tags = "browser, web, javascript, chrome, chromium, flash, webgl")]
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

    [PluginInfo(Name = "HTMLView", 
                Category = "Image", 
                Version = "URL", 
                Credits = "Development sponsored by http://meso.net",
                Tags = "browser, web, javascript, chrome, chromium, flash, webgl")]
    public class HTMLTextureUrlNode : HTMLTextureNode
    {
        [Input("Url", DefaultString = HTMLTextureRenderer.DEFAULT_URL)]
        public ISpread<string> FUrlIn;

        protected override void LoadContent(HTMLTextureRenderer renderer, Size size, int slice)
        {
            renderer.LoadUrl(size, FUrlIn[slice]);
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
        [Input("Frame Rate", MinValue = HTMLTextureRenderer.MIN_FRAME_RATE, MaxValue = HTMLTextureRenderer.MAX_FRAME_RATE, DefaultValue = 60)]
        public ISpread<int> FFrameRateIn;
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
        public ISpread<IObservable<IImage>> FImageOut;
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
        [Output("Loaded")]
        public ISpread<bool> FLoadedOut;
        [Output("Current Url")]
        public ISpread<string> FCurrentUrlOut;
        [Output("Error Text")]
        public ISpread<string> FErrorTextOut;
        [Output("On Data")]
        public ISpread<bool> FOnDataOut;
        [Output("Data")]
        public ISpread<XElement> FDataOut;

        [Import]
        protected ILogger FLogger;

        private readonly Spread<HTMLTextureRenderer> FWebRenderers = new Spread<HTMLTextureRenderer>();

        public virtual void OnImportsSatisfied()
        {
            FImageOut.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            FWebRenderers.ResizeAndDispose(spreadMax, (i) => new HTMLTextureRenderer(FLogger, FFrameRateIn[i]));

            FImageOut.SliceCount = spreadMax;
            FRootElementOut.SliceCount = spreadMax;
            FDomOut.SliceCount = spreadMax;
            FDocumentWidthOut.SliceCount = spreadMax;
            FDocumentHeightOut.SliceCount = spreadMax;
            FIsLoadingOut.SliceCount = spreadMax;
            FLoadedOut.SliceCount = spreadMax;
            FErrorTextOut.SliceCount = spreadMax;
            FCurrentUrlOut.SliceCount = spreadMax;
            FOnDataOut.SliceCount = spreadMax;
            FDataOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var webRenderer = FWebRenderers[i];

                var frameRate = VMath.Clamp(FFrameRateIn[i], HTMLTextureRenderer.MIN_FRAME_RATE, HTMLTextureRenderer.MAX_FRAME_RATE);
                if (frameRate != webRenderer.FrameRate)
                {
                    webRenderer.Dispose();
                    webRenderer = new HTMLTextureRenderer(FLogger, frameRate);
                    FWebRenderers[i] = webRenderer;
                }

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
                FImageOut[i] = webRenderer.Images;
                FErrorTextOut[i] = webRenderer.CurrentError;
                FIsLoadingOut[i] = webRenderer.IsLoading;
                FLoadedOut[i] = webRenderer.Loaded;
                // As long as the renderer is in the loading state stick to the old values
                if (!webRenderer.IsLoading)
                {
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
                    FCurrentUrlOut[i] = webRenderer.CurrentUrl;
                }

                XElement outputs;
                if (webRenderer.TryReceive(out outputs))
                {
                    FDataOut[i] = outputs;
                    FOnDataOut[i] = true;
                }
                else
                    FOnDataOut[i] = false;
            }
        }

        protected abstract void LoadContent(HTMLTextureRenderer renderer, Size size, int slice);

        public void Dispose()
        {
            foreach (var renderer in FWebRenderers)
                renderer.Dispose();
        }
    }
}
