#region usings
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Win32;

#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "WindowFrame", Category = "Windows", Help = "Toggles the frame of the active window", AutoEvaluate = true)]
    #endregion PluginInfo
    public class WindowFrame : IPluginEvaluate
    {
        #region fields & pins
        [Input("Handle", DefaultValue = 1.0)]
        public IDiffSpread<int> FHandle;

        [Input("Apply", DefaultValue = 0.0, AutoValidate = true)]
        public IDiffSpread<bool> FApply;

        #endregion fields & pins

        #region USER32 functions import

        [DllImport("User32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int Width, int Height, bool Repaint);

        [DllImport("User32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int width, int height, int flags);

        [DllImport("User32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("User32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private static int GWL_STYLE = -16;
        private static int GWL_EXSTYLE = -20;

        private static int WS_CAPTION = 0x00C00000;
        private static int WS_THICKFRAME = 0x00040000;
        private static int WS_MINIMIZE = 0x20000000;
        private static int WS_MAXIMIZE = 0x01000000;
        private static int WS_SYSMENU = 0x00080000;

        private static int WS_EX_DLGMODALFRAME = 0x00000001;
        private static int WS_EX_CLIENTEDGE = 0x00000200;
        private static int WS_EX_STATICEDGE = 0x00020000;

        private static int SWP_FRAMECHANGED = 0x0020;
        private static int SWP_NOMOVE = 0x0002;
        private static int SWP_NOSIZE = 0x0001;
        private static int SWP_NOZORDER = 0x0004;
        private static int SWP_NOOWNERZORDER = 0x0200;

        #endregion USER32 functions import

        private IntPtr hWnd;
        private RECT client, window;
        private int paddingX, paddingY;
        private int style, extendedStyle;

        //Small object holding the Styles of the window.
        public struct WindowInfo
        {
            public int style { get; set; }
            public int extendedStyle { get; set; }
        }

        //Dictionary: from window handle to styles.
        private Dictionary<IntPtr, WindowInfo> styles;


        public WindowFrame()
        {
            styles = new Dictionary<IntPtr, WindowInfo>();
            hWnd = System.IntPtr.Zero;
        }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            //Clear the dictionary if the handles are changed.
            if (FHandle.IsChanged)
            {
                styles.Clear();
            }

            if (FApply.IsChanged)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    hWnd = (IntPtr)FHandle[i];

                    if (IsWindow(hWnd))
                    {
                        if (FApply[i])
                        {
                            // Get Window and Client Rectangles
                            GetClientRect(hWnd, out client);
                            GetWindowRect(hWnd, out window);

                            //Calculate Paddings (innerspace between Client and Window)
                            paddingX = window.Right - window.Left - client.Right;
                            paddingY = window.Bottom - window.Top - client.Bottom;

                            //Ask for the Style and Extended Styles of the Window
                            style = GetWindowLong(hWnd, GWL_STYLE);
                            extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                            
                            //Create, Fill and write to a List an object containing the Styles.
                            var info = new WindowInfo();
                            info.style = style;
                            info.extendedStyle = extendedStyle;

                            if (styles.ContainsKey(hWnd))
                            {
                                styles.Remove(hWnd);
                            }

                            styles.Add(hWnd, info);

                            //Inverse these styles removes the title bar and other junk
                            style &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZE | WS_MAXIMIZE | WS_SYSMENU);
                            extendedStyle &= ~(WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE);
                            
                            //Sets the new styles.
                            SetWindowLong(hWnd, GWL_STYLE, style);
                            SetWindowLong(hWnd, GWL_EXSTYLE, extendedStyle);

                            //Update Window's position and the size. Now without Titlebar and so on.
                            //Only the Client area is visible now.
                            MoveWindow(hWnd, window.Left + paddingX / 2, window.Top, client.Right, client.Bottom, true);
                        }
                        else
                        {
                            var info = new WindowInfo();

                            //If there is a Window in the List, then we can restore its Style.
                            if (styles.TryGetValue(hWnd, out info))
                            {
                                
                                //Get the current window's Rect.
                                RECT currentPos = new RECT();
                                GetWindowRect(hWnd, out currentPos);

                                //Get the saved styles
                                style = info.style;
                                extendedStyle = info.extendedStyle;

                                //Set the styles.
                                SetWindowLong(hWnd, GWL_STYLE, style);
                                SetWindowLong(hWnd, GWL_EXSTYLE, extendedStyle);

                                //Update the window to apply the styles.
                                SetWindowPos(hWnd, (IntPtr)0, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOZORDER | SWP_NOOWNERZORDER);

                                //Get the current window's and client area Rects.
                                //Now after applying the styles.
                                GetClientRect(hWnd, out client);
                                GetWindowRect(hWnd, out window);

                                //Calculate the paddings.
                                paddingX = window.Right - window.Left - client.Right;
                                paddingY = window.Bottom - window.Top - client.Bottom;

                                //Update window position and the size.
                                //Now with the Titlebar again.
                                MoveWindow(hWnd, currentPos.Left - paddingX / 2, currentPos.Top, currentPos.Width + paddingX, currentPos.Height + paddingY, true);
                            }
                        }
                    }
                }

            }
        }
    }

}