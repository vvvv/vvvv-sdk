using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;
using Xilium.CefGlue;

namespace VVVV.Nodes.Texture.HTML
{
    [Startable]
    public class HTMLTextureStartable : IStartable
    {
        // Main entry point when called by vvvv
        void IStartable.Start()
        {
            CefRuntime.Load();

            var cefSettings = new CefSettings();
            cefSettings.WindowlessRenderingEnabled = true;
            cefSettings.PackLoadingDisabled = false;
            cefSettings.MultiThreadedMessageLoop = true;
            cefSettings.BrowserSubprocessPath = Assembly.GetExecutingAssembly().Location;
            cefSettings.CommandLineArgsDisabled = false;
            cefSettings.IgnoreCertificateErrors = true;
            //// We do not meet the requirements - see cef_sandbox_win.h
            //cefSettings.NoSandbox = true;
#if DEBUG
            cefSettings.LogSeverity = CefLogSeverity.Error;
            // Set to true to debug DOM / JavaScript
            cefSettings.SingleProcess = false;
#else
            cefSettings.LogSeverity = CefLogSeverity.Disable;
#endif

            var args = Environment.GetCommandLineArgs();
            var mainArgs = new CefMainArgs(args);
            CefRuntime.Initialize(mainArgs, cefSettings, new HTMLTextureApp(), IntPtr.Zero);

            var schemeName = "cef";
            if (!CefRuntime.RegisterSchemeHandlerFactory(SchemeHandlerFactory.SCHEME_NAME, null, new SchemeHandlerFactory()))
                throw new Exception(string.Format("Couldn't register custom scheme factory for '{0}'.", schemeName));
        }

        void IStartable.Shutdown()
        {
            CefRuntime.Shutdown();
        }
    }
}
