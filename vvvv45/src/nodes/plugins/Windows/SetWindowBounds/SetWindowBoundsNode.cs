#region usings
using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Win32;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "SetWindowBounds", Category = "Windows", Help = "Sets the client size and position of a given window handle in pixels.", AutoEvaluate = true)]
    #endregion PluginInfo
    public class SetWindowBounds : IPluginEvaluate
    {
        #region fields & pins
        [Input("Handle", DefaultValue = 1.0)]
        public ISpread<int> FHandle;

        [Input("X", DefaultValue = 0)]
        public ISpread<int> FXPos;

        [Input("Y", DefaultValue = 0)]
        public ISpread<int> FYPos;

        [Input("Width", DefaultValue = 420)]
        public ISpread<int> FWidth;

        [Input("Height", DefaultValue = 42)]
        public ISpread<int> FHeight;

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

        public SetWindowBounds()
        {
            hWnd = System.IntPtr.Zero;
            client = new RECT();
            window = new RECT();
        }


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
                }

                if (FSetSize[i] && IsWindow (hWnd))
                {
                    var paddingX = window.Right - window.Left - client.Right;
                    var paddingY = window.Bottom - window.Top - client.Bottom;

                    MoveWindow(hWnd, window.Left, window.Top, FWidth[i] + paddingX, FHeight[i] + paddingY, true);
                }

                if (FSetPosition[i] && IsWindow(hWnd))
                {
                    MoveWindow(hWnd, FXPos[i] - client.Left, FYPos[i] - client.Top, window.Width, window.Height, true);
                }

            }



        }

    }
}
