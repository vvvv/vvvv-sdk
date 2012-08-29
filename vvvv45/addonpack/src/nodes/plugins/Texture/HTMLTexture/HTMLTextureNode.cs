using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using SlimDX.Direct3D9;
using CefGlue;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;
using System.ComponentModel.Composition;
using VVVV.Utils.IO;
using System.Reflection;
using System.IO;
using System.Xml.Linq;
using EX9 = SlimDX.Direct3D9;

namespace VVVV.Nodes.Texture.HTML
{
    [PluginInfo(Name = "HTMLTexture", Category = "EX9.Texture", Version = "Chromium", Tags = "browser, web, html, javascript, chromium, flash, webgl")]
    public class HTMLTextureNode : IPluginEvaluate, IDisposable, IPartImportsSatisfiedNotification
    {
        static HTMLTextureNode()
        {
            var pathToThisAssembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathToBinFolder = Path.Combine(pathToThisAssembly, "Dependencies", "CefGlue");
            var envPath = Environment.GetEnvironmentVariable("PATH");
            envPath = string.Format("{0};{1}", envPath, pathToBinFolder);
            Environment.SetEnvironmentVariable("PATH", envPath);
        }

        [Input("Url", DefaultString = HTMLTextureRenderer.DEFAULT_URL)]
        public ISpread<string> FUrlIn;
        [Input("HTML", DefaultString = "")]
        public ISpread<string> FHtmlIn;
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
                var url = FUrlIn[i];
                var html = FHtmlIn[i];
                var reload = FReloadIn[i];
                var width = FWidthIn[i];
                var height = FHeightIn[i];
                var zoomLevel = FZoomLevelIn[i];
                var mouseEvent = FMouseEventIn[i];
                var keyEvent = FKeyEventIn[i];
                var scrollTo = FScrollToIn[i];
                var javaScript = FJavaScriptIn[i];
                var execute = FExecuteIn[i];
                var enabled = FEnabledIn[i];
                XDocument dom; 
                XElement rootElement;
                bool isLoading;
                string currentUrl, errorText;
                var output = webRenderer.Render(
                    out dom,
                    out rootElement,
                    out isLoading, 
                    out currentUrl, 
                    out errorText, 
                    url, 
                    html, 
                    reload,
                    width, 
                    height, 
                    zoomLevel,
                    mouseEvent,
                    keyEvent,
                    scrollTo,
                    javaScript,
                    execute,
                    enabled);
                if (FOutput[i] != output) FOutput[i] = output;
                if (FDomOut[i] != dom) FDomOut[i] = dom;
                if (FRootElementOut[i] != rootElement) FRootElementOut[i] = rootElement;
                FIsLoadingOut[i] = isLoading;
                FCurrentUrlOut[i] = currentUrl;
                FErrorTextOut[i] = errorText;
            }
        }

        public void Dispose()
        {
            FWebRenderers.ResizeAndDispose(0, () => new HTMLTextureRenderer(FLogger));
        }
    }
}
