#region usings
using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Win32;

using SlimDX;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "GetWindowBounds", Category = "Windows ClientArea", Help = "Returns the size and position of a window's client area (ie. without border and titlebar) in pixels.", AutoEvaluate = false)]
    #endregion PluginInfo
    public class GetWindowBoundsClient : IPluginEvaluate
    {
        #region fields & pins
        [Input("Handle", DefaultValue = 1.0)]
        public ISpread<int> FHandle;

        [Output("Position")]
        public ISpread<Vector2> FPosition;

        [Output("Size")]
        public ISpread<Vector2> FSize;

        #endregion fields & pins

        #region USER32 functions import

        [DllImport("User32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern int GetSystemMetrics(int index);

        const int CXFRAME = 0x20;       // Border X
        const int CYFRAME = 0x21;       // Border Y
        const int CYSMCAPTION = 0x33;   // Caption

        #endregion USER32 functions import

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            FSize.SliceCount = FPosition.SliceCount = SpreadMax;

            if (FHandle.IsChanged)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    IntPtr hWnd = (IntPtr)FHandle[i];

                    if (IsWindow(hWnd))
                    {
                        RECT client, window;
                        GetClientRect(hWnd, out client);
                        GetWindowRect(hWnd, out window);

                        var borderX = GetSystemMetrics(CXFRAME);
                        var borderY = GetSystemMetrics(CYFRAME);
                        var titleBarSize = GetSystemMetrics(CYSMCAPTION);

                        FPosition[i] = new Vector2(window.Left + borderX, window.Top + borderY + titleBarSize);
                        FSize[i] = new Vector2(client.Right, client.Bottom);
                    }
                }

            }
        }

    }
}
