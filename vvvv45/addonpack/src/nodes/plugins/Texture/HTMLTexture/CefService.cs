﻿using System;
using CefGlue;
using VVVV.PluginInterfaces.V2;
using System.IO;
using System.Reflection;

namespace VVVV.Nodes.Texture.HTML
{
    /// <summary>
    /// Starts and stops mainloop of CEF.
    /// </summary>
    [Startable]
    public class CefService : IStartable
    {
        public void Start()
        {
            var pathToThisAssembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathToBinFolder = Path.Combine(pathToThisAssembly, "Dependencies", "x86", "CefGlue");
            var envPath = Environment.GetEnvironmentVariable("PATH");
            envPath = string.Format("{0};{1}", envPath, pathToBinFolder);
            Environment.SetEnvironmentVariable("PATH", envPath);

            var cefSettings = new CefSettings();
            cefSettings.GraphicsImplementation = CefGraphicsImplementation.DesktopInProcess;
            cefSettings.MultiThreadedMessageLoop = true;
            //cefSettings.ExtraPluginPaths.Add(@"D:\vvvv_dev\misc\cef_binary_1.1025.607_windows\Release\pdf.dll");
            //cefSettings.ExtraPluginPaths.Add(@"D:\vvvv_dev\misc\cef_binary_1.1025.607_windows\Release\gcswf32.dll");

            //            cefSettings.CachePath = Path.GetDirectoryName(Application.ExecutablePath) + "/cache";
            //            cefSettings.LogFile = Path.GetDirectoryName(Application.ExecutablePath) + "/CEF.log";
            // Disable logging as it leads to freezes and crashes in some occasions.
            // For example see: 
            // * http://code.google.com/p/chromiumembedded/issues/detail?id=570
            // * http://www.magpcss.org/ceforum/viewtopic.php?f=6&t=761
            cefSettings.LogSeverity = CefLogSeverity.Disable;
            Cef.Initialize(cefSettings);
            CefGlue.Threading.CefThread.UI.Send(_ => System.Threading.Thread.CurrentThread.Name = "UI", null);
            CefGlue.Threading.CefThread.IO.Send(_ => System.Threading.Thread.CurrentThread.Name = "IO", null);
            CefGlue.Threading.CefThread.File.Send(_ => System.Threading.Thread.CurrentThread.Name = "File", null);

            var schemeName = "cef";
            if (!Cef.RegisterCustomScheme(schemeName, false, true, false)) throw new Exception(string.Format("Couldn't register custom scheme '{0}'.", schemeName));
            if (!Cef.RegisterSchemeHandlerFactory(schemeName, null, new SchemeHandlerFactory())) throw new Exception(string.Format("Couldn't register custom scheme factory for '{0}'.", schemeName));
        }

        public void Shutdown()
        {
            Cef.Shutdown();
        }
    }
}
