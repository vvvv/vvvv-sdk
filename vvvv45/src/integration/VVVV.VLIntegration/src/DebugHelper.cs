using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

// See http://stackoverflow.com/questions/4451132/visual-studio-breakpoints-break-on-specific-file-access
static class DebugHelper
{
    public static void BreakOnFileAccess(string path)
    {
        var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        new Thread(() => {
            while (true)
            {
                Thread.Sleep(Timeout.Infinite);
                // ensure FileStream isn't GC'd as a local variable after its last usage
                GC.KeepAlive(fs);
            }
        }).Start();
    }
}