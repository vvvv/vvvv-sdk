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
    public class SetWindowSize : IPluginEvaluate
    {
        #region fields & pins
        [Input("Handle", DefaultValue = 1.0)]
        public ISpread<int> FHandle;

        [Input("Width", DefaultValue = 420)]
        public ISpread<int> FWidth;

        [Input("Height", DefaultValue = 42)]
        public ISpread<int> FHeight;

        [Input("Apply", DefaultValue = 0.0, IsBang = true)]
        public IDiffSpread<bool> FApply;
        #endregion fields & pins

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                if (FApply[i])
                {
                    IntPtr hWnd = (IntPtr) FHandle[i];
                    RECT rcClient, rcWindow;

                    if (IsWindow(hWnd))
                    {

                        User32.GetClientRect(hWnd, out rcClient);
                        User32.GetWindowRect(hWnd, out rcWindow);

                        var x = rcWindow.Right - rcWindow.Left - rcClient.Right;
                        var y = rcWindow.Bottom - rcWindow.Top - rcClient.Bottom;

                        MoveWindow(hWnd, rcWindow.Left, rcWindow.Top, FWidth[i] + x, FHeight[i] + y, true);
                    }
                }

            }

        }

    }
}
