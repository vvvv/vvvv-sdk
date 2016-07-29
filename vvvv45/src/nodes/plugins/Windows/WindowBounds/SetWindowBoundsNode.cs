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
    [PluginInfo(Name = "SetWindowBounds", Category = "Windows", Help = "Sets the size and position of a given window in pixels including its border and titlebar.", AutoEvaluate = true)]
    #endregion PluginInfo
    public class SetWindowBounds : IPluginEvaluate
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
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        #endregion USER32 functions import

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                if (FSetSize[i] || FSetPosition[i])
                {
                    IntPtr hWnd = (IntPtr)FHandle[i];

                    RECT window;
                    GetWindowRect(hWnd, out window);

                    if (FSetSize[i] && IsWindow(hWnd))
                    {
                        MoveWindow(hWnd, window.Left, window.Top, (int)FSize[i].X, (int)FSize[i].Y, true);
                    }

                    if (FSetPosition[i] && IsWindow(hWnd))
                    {
                        MoveWindow(hWnd, (int)FPos[i].X, (int)FPos[i].Y, window.Width, window.Height, true);
                    }

                }

            }



        }

    }
}
