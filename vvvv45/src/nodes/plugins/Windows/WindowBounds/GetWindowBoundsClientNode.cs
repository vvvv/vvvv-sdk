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
    [PluginInfo(Name = "GetWindowBounds", Category = "Windows ClientArea", Help = "Returns position and the size of a window's client area (without borders, shadows etc.) in pixels.", AutoEvaluate = false)]
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

        private IntPtr hWnd;
        private RECT client;
        private RECT window;

        [DllImport("User32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        public GetWindowBoundsClient()
        {
            hWnd = System.IntPtr.Zero;
            client = new RECT();
            window = new RECT();
        }


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            FSize.SliceCount = FPosition.SliceCount = SpreadMax;

            if (FHandle.IsChanged)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    hWnd = (IntPtr)FHandle[i];

                    if (IsWindow(hWnd))
                    {
                        GetClientRect(hWnd, out client);
                        GetWindowRect(hWnd, out window);

                        var paddingX = window.Right - window.Left - client.Right;
                        var paddingY = window.Bottom - window.Top - client.Bottom;
                        
                        FPosition[i] = new Vector2(window.Left + paddingX / 2, window.Top);

                        FSize[i] = new Vector2(client.Right, client.Bottom);
                    }
                }

            }
        }

    }
}
