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
    [PluginInfo(Name = "SetWindowBounds", Category = "Windows ClientArea", Help = "Sets position and the size of a window's client area (inner area without borders, shadows etc.) in pixels.", AutoEvaluate = true)]
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

        private IntPtr hWnd;
        private RECT client, window;

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("User32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                if (FSetSize[i] || FSetPosition[i])
                {
                    hWnd = (IntPtr)FHandle[i];

                    GetClientRect(hWnd, out client);
                    GetWindowRect(hWnd, out window);

                    var paddingX = window.Right - window.Left - client.Right;
                    var paddingY = window.Bottom - window.Top - client.Bottom;

                    if (FSetSize[i] && IsWindow(hWnd))
                    {
                        MoveWindow(hWnd, window.Left, window.Top, (int)FSize[i].X + paddingX, (int)FSize[i].Y + paddingY, true);
                    }

                    if (FSetPosition[i] && IsWindow(hWnd))
                    {
                        MoveWindow(hWnd, (int)FPos[i].X - paddingX/2, (int)FPos[i].Y, window.Width, window.Height, true);
                    }

                }

            }



        }

    }
}
