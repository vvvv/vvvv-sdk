using System;
using CefGlue;

namespace VVVV.Nodes.HTML
{
    /// <summary>
    /// Starts and stops mainloop of CEF.
    /// </summary>
    static class CefService
    {
        private static int FRefCount;
        
        public static void AddRef()
        {
            if (FRefCount == 0)
            {
                var cefSettings = new CefSettings();
                cefSettings.MultiThreadedMessageLoop = true;
                //            cefSettings.CachePath = Path.GetDirectoryName(Application.ExecutablePath) + "/cache";
                //            cefSettings.LogFile = Path.GetDirectoryName(Application.ExecutablePath) + "/CEF.log";
                //            cefSettings.LogSeverity = CefLogSeverity.Verbose;
                Cef.Initialize(cefSettings);
            }
            FRefCount++;
        }
        
        public static void Release()
        {
            FRefCount--;
            if (FRefCount == 0)
            {
                Cef.Shutdown();
            }
        }
    }
}
