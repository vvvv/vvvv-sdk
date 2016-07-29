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
    [PluginInfo(Name = "SetWindowBounds", Category = "Windows ClientArea", Help = "Sets the size and position of a window's client area (ie. without border and titlebar) in pixels.", AutoEvaluate = true)]
    #endregion PluginInfo
    public class SetWindowBoundsClient : IPluginEvaluate
    {
        #region fields & pins
        [Input("Handle", DefaultValue = 1.0)]
        public ISpread<int> FHandle;

        [Input("Position")]
        public ISpread<Vector2> FPos;

        [Input("Size")]
        public ISpread<Vector2> FSize;

        [Input("Set Position", DefaultValue = 0.0, IsBang = true)]
        public ISpread<bool> FSetPosition;

        [Input("Set Size", DefaultValue = 0.0, IsBang = true)]
        public ISpread<bool> FSetSize;

        #endregion fields & pins

        #region USER32 functions import

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

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
            for (int i = 0; i < SpreadMax; i++)
            {
                if (FSetSize[i] || FSetPosition[i])
                {
                    IntPtr hWnd = (IntPtr)FHandle[i];

                    RECT client, window;
                    GetClientRect(hWnd, out client);
                    GetWindowRect(hWnd, out window);

                    var borderX = GetSystemMetrics(CXFRAME);
                    var borderY = GetSystemMetrics(CYFRAME);
                    var titleBarSize = GetSystemMetrics(CYSMCAPTION);

                    if (FSetSize[i] && IsWindow(hWnd))
                    {
                        MoveWindow(hWnd, window.Left, window.Top, (int)FSize[i].X + borderX * 2, (int)FSize[i].Y + borderY * 2 + titleBarSize, true);
                    }

                    if (FSetPosition[i] && IsWindow(hWnd))
                    {
                        MoveWindow(hWnd, (int)FPos[i].X - borderX, (int)FPos[i].Y - borderY - titleBarSize, window.Width, window.Height, true);
                    }

                }

            }



        }

    }
}
