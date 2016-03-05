#region licence/info

//////project name
//SetWindowSize (Windows)

//////description
//Sets size of the window.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V2;

//////initial author
//anton

#endregion licence/info

#region usings
using System;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Win32;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "SetWindowSize", Category = "Windows", Help = "Sets the size of the window.", AutoEvaluate = true)]
    #endregion PluginInfo
    public class SetWindowSize : IPluginEvaluate
    {
        #region fields & pins
        [Input("WindowHandle", DefaultValue = 1.0)]
        public ISpread<int> FHandle;

        [Input("Width", DefaultValue = 420)]
        public ISpread<int> FWidth;

        [Input("Height", DefaultValue = 42)]
        public ISpread<int> FHeight;

        [Input("Set", DefaultValue = 1.0)]
        public IDiffSpread<bool> FSet;
        #endregion fields & pins

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                if (FSet[i])
                {
                    IntPtr hWnd = (IntPtr) FHandle[i];

                    RECT rcClient, rcWindow;
                    if (User32.GetClientRect(hWnd, out rcClient))
                    {
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
