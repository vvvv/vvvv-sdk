using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
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
        /// <param name="addCursor">Render the cursor into the image</param>
        /// <returns></returns>
        public Image CaptureWindow(IntPtr handle, bool addCursor = true)
        {
            // get the size
            RECT windowRect;
            User32.GetWindowRect(handle, out windowRect);

            const int CXFRAME = 0x20;
            const int CYFRAME = 0x21;
            const int CYSMCAPTION = 0x33;

            var cdX = User32.GetSystemMetrics(CXFRAME);
            var cdY = User32.GetSystemMetrics(CYFRAME);
            var cdC = User32.GetSystemMetrics(CYSMCAPTION);

            var left = windowRect.Left + cdX;
            var top = windowRect.Top + cdY + cdC;
            var width = windowRect.Width - 2 * cdX;
            var height = windowRect.Height - (2 * cdY + cdC);

            var bmp = new Bitmap(width, height);

            using (var g = Graphics.FromImage(bmp))
            {
                //capture desktop
                g.CopyFromScreen(left, top, 0, 0, new Size(width, height));

                //draw cursor
                if (addCursor && Cursor.Current != null)
                {
                    var x = Cursor.Position.X - windowRect.Left - cdX - Cursor.Current.HotSpot.X;
                    var y = Cursor.Position.Y - windowRect.Top - cdY - cdC - Cursor.Current.HotSpot.Y;
                    var rect = new Rectangle(new Point(x, y), Cursor.Current.Size);
                    Cursor.Current.Draw(g, rect);
                }
            }

            return bmp;
        }
    }
}
