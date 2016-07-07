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
    [PluginInfo(Name = "GetWindowBounds", Category = "Windows", Help = "Returns the client size and position of a given window handle in pixels.", AutoEvaluate = true)]
    #endregion PluginInfo
    public class GetWindowBounds : IPluginEvaluate
    {
        #region fields & pins
        [Input("Handle", DefaultValue = 1.0)]
        public ISpread<int> FHandle;

        [Output("Position")]
        public ISpread<Vector2> FPosition;

        [Output("Width")]
        public ISpread<int> FWidth;

        [Output("Height")]
        public ISpread<int> FHeight;

        #endregion fields & pins

        private IntPtr hWnd;
        private RECT client;
        private RECT window;

        [DllImport("User32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        public GetWindowBounds()
        {
            hWnd = System.IntPtr.Zero;
            client = new RECT();
            window = new RECT();
        }


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            FWidth.SliceCount = FHeight.SliceCount = FPosition.SliceCount = SpreadMax;

            if (FHandle.IsChanged)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    hWnd = (IntPtr)FHandle[i];

                    if (IsWindow(hWnd))
                    {
                        GetClientRect(hWnd, out client);
                        GetWindowRect(hWnd, out window);

                        FPosition[i] = new Vector2(window.Left, window.Top);
                        FWidth[i] = client.Right;
                        FHeight[i] = client.Bottom;
                    }
                }

            }
        }

    }
}
