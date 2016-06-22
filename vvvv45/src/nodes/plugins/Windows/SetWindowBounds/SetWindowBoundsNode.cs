#region usings
using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Win32;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "SetWindowSize", Category = "Windows", Help = "Sets the client size of a given window handle in pixels.", AutoEvaluate = true)]
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
        public IDiffSpread<bool> FSetPosition;

        [Input("Set Size", DefaultValue = 0.0, IsBang = true)]
        public IDiffSpread<bool> FSetSize;

        #endregion fields & pins

        private IntPtr hWnd;
        private RECT rcClient, rcWindow;
        private int paddingX, paddingY;

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("User32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        public SetWindowBounds()
        {
            hWnd = System.IntPtr.Zero;
            rcClient = new RECT();
            rcWindow = new RECT();

            paddingX = 0;
            paddingY = 0;
        }


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            

            for (int i = 0; i < SpreadMax; i++)
            {

                if (FSetSize[i] || FSetPosition[i])
                {
                    hWnd = (IntPtr)FHandle[i];

                    User32.GetClientRect(hWnd, out rcClient);
                    User32.GetWindowRect(hWnd, out rcWindow);

                    paddingX = rcWindow.Right - rcWindow.Left - rcClient.Right;
                    paddingY = rcWindow.Bottom - rcWindow.Top - rcClient.Bottom;
                }

                if (FSetSize[i])
                {
                    MoveWindow(hWnd, rcWindow.Left, rcWindow.Top, FWidth[i] + paddingX, FHeight[i] + paddingY, true);
                }

                if (FSetPosition[i])
                {
                   MoveWindow(hWnd, FXPos[i]+ paddingX, FYPos[i]+ paddingY, rcClient.Width, rcClient.Height, true);
                }

            }

        }

    }
}
