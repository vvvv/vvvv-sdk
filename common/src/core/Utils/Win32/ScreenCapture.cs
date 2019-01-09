using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Utils.Win32
{
    //adapted from:
    //http://stackoverflow.com/questions/5049122/capture-the-screen-shot-using-net

    public class ScreenCapture
    {
        /// <summary>
        /// Creates an Image object containing a screen shot of the entire desktop
        /// </summary>
        /// <returns></returns>
        public Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }
        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. 
        /// (In windows forms, this is obtained by the Handle property)</param>
        /// <param name="removeBorder">Remove window Borders</param>
        /// <param name="rect">Defines a subrect of the given window handle to capture</param>
        /// <param name="addCursor">Render the cursor into the image</param>
        /// <returns></returns>
        public Image CaptureWindow(IntPtr handle, bool removeBorders = true, Rectangle? subrect = null, bool addCursor = true, Point? cursorPos = null)
        {
            // get the size
            RECT windowRect;
            User32.GetWindowRect(handle, out windowRect);

            const int CXFRAME = 0x20;
            const int CYFRAME = 0x21;
            const int CYSMCAPTION = 0x33;

            int cdX = 0;
            int cdY = 0;
            int cdC = 0;
            var left = windowRect.Left;
            var top = windowRect.Top;
            var width = windowRect.Width;
            var height = windowRect.Height;

            if (removeBorders)
            {
                cdX = User32.GetSystemMetrics(CXFRAME);
                cdY = User32.GetSystemMetrics(CYFRAME);
                
                //windows with empty captions don't have a titlebar (like the VL editor)
                if (User32.GetWindowTextLength(handle) != 0)
                    cdC = User32.GetSystemMetrics(CYSMCAPTION);

                left += cdX;
                top += cdY + cdC;
                width -= (2 * cdX);
                height -= (2 * cdY + cdC);
            }

            if (subrect.HasValue)
            {
                left += Math.Max(0, subrect.Value.Left);
                top += Math.Max(0, subrect.Value.Top);
                width = Math.Min(width, subrect.Value.Width);
                height = Math.Min(height, subrect.Value.Height);
            }

            var bmp = new Bitmap(width, height);

            using (var g = Graphics.FromImage(bmp))
            {
                //capture desktop
                g.CopyFromScreen(left, top, 0, 0, new Size(width, height));

                //draw cursor
                if (addCursor && Cursor.Current != null)
                {
                    var x = Cursor.Position.X - left;
                    var y = Cursor.Position.Y - top;
                    if (cursorPos.HasValue)
                    {
                        x = cursorPos.Value.X;
                        y = cursorPos.Value.Y;
                    }
                    x -= Cursor.Current.HotSpot.X;
                    y -= Cursor.Current.HotSpot.Y;
                    var cursorRect = new Rectangle(new Point(x, y), Cursor.Current.Size);

                    var capturedRect = new Rectangle(0, 0, width, height);
                    if (capturedRect.Contains(cursorRect))
                        Cursor.Current.Draw(g, cursorRect);
                }
            }

            return bmp;
        }
    }
}
