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
    [PluginInfo(Name = "GetWindowBounds", Category = "Windows", Help = "Returns the size and position of a given window in pixels including its border and titlebar.", AutoEvaluate = false)]
    #endregion PluginInfo
    public class GetWindowBounds : IPluginEvaluate
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
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

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
                        RECT window;
                        GetWindowRect(hWnd, out window);

                        FPosition[i] = new Vector2(window.Left, window.Top);
                        FSize[i] = new Vector2(window.Width, window.Height);
                    }
                }

            }
        }

    }
}
