#region usings
using System;
using System.Threading.Tasks;
using System.IO;

using VVVV.Utils.Win32;
using VVVV.Utils.Imaging;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
#endregion usings

namespace VVVV.Utils.Win32
{
    public enum CaptureState { Idle, Capturing, Writing }

    struct Frame
    {
        public Image Image;
        public float Delay;
    }

    #region GifStuff
    public enum GifDitherType
    {
        None = 0,
        Solid = 1,
        Ordered4x4 = 2,
        Ordered8x8 = 3,
        Ordered16x16 = 4,
        Spiral4x4 = 5,
        Spiral8x8 = 6,
        DualSpiral4x4 = 7,
        DualSpiral8x8 = 8,
        ErrorDiffusion = 9,
    }
    public enum GifPaletteType
    {
        Optimal = 1,
        FixedBW = 2,
        FixedHalftone8 = 3,
        FixedHalftone27 = 4,
        FixedHalftone64 = 5,
        FixedHalftone125 = 6,
        FixedHalftone216 = 7,
        FixedHalftone252 = 8,
        FixedHalftone256 = 9
    }
    #endregion

    public class ScreenRecorder: IDisposable
    {
        #region fields
        private Timer FTimer = new Timer();

        private IntPtr FHandle;
        private bool FShowCursor;
        private Rectangle? FSubrect;
        private int FDelayInMilliseconds;

        public bool ReportProgress = true;

        public int FrameCountOut;

        public bool FRecord;
        public int FColorCount;
        public bool FIsRecording;
        public int FFrameCountOut;
        public bool FIsWriting;
        public string FLastFile;

        double FLastFrameTime;
        string FSaveFilePath;

        CaptureState FCaptureState;
        ScreenCapture FScreenCapture = new ScreenCapture();
        List<Frame> FFrames = new List<Frame>();

        Gif FGIF = new Gif();
        ProgressForm FProgressBar = new ProgressForm();
        Label FProgressLabel = new Label();
        double FCurrentTime;
        #endregion fields & pins

        public class ProgressForm : Form
        {
            //to not steal keyboard focus
            //as we need to turn of recording via keyboard again
            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }
        }
        
        public ScreenRecorder()
        {
            FProgressBar.FormBorderStyle = FormBorderStyle.None;
            FProgressBar.Size = new Size(0, 0);
            //don't topmost as this would again activate the form!
            //FProgressBar.TopMost = true;

            FProgressLabel.Padding = new Padding(4);
            FProgressLabel.AutoSize = true;
            FProgressLabel.Parent = FProgressBar;

            FTimer.Tick += Snap;
        }

        public void Dispose()
        {
            FProgressLabel.Dispose();
            FProgressBar.Dispose();

            FTimer.Stop();
            FTimer.Tick -= Snap;
            FTimer.Dispose();
        }

        public bool Start(IntPtr handle, int fps = 25, Rectangle? subrect = null, bool showCursor = true)
        {
            if (FCaptureState != CaptureState.Idle)
                return false;

            FHandle = handle;
            FShowCursor = showCursor;
            FSubrect = subrect;
            FCaptureState = CaptureState.Capturing;
            FDelayInMilliseconds = 1000 / fps;

            FTimer.Interval = FDelayInMilliseconds;
            FTimer.Start();

            return true;
        }

        private void Snap(object sender, EventArgs e)
        {
            try
            {
                var img = FScreenCapture.CaptureWindow(FHandle, false, FSubrect, FShowCursor);

                FFrames.Add(new Frame() { Image = img, Delay = FTimer.Interval });
            }
            catch (Exception exception)
            {
                //in case of exception assume out of memory (even if no OOM exception occurs)
                //free some memory by removing the last recorded frames for Saving not to fail
                var freeCount = Math.Min(FFrames.Count, 10);
                for (int i = FFrames.Count - freeCount; i < FFrames.Count; i++)
                    FFrames[i].Image.Dispose();
                FFrames = FFrames.SkipLast(freeCount).ToList();

                //then stop
                Stop();
            }

            if (ReportProgress)
                DoReportProgress();
        }

        public void Stop()
        {
            if (FCaptureState != CaptureState.Capturing)
                return;

            FTimer.Stop();
            FCaptureState = CaptureState.Idle;
            DoReportProgress();
        }

        public async void SaveGIFAsync(string filepath, PaletteType paletteType, DitherType dithertype, int colorCount = 256)
        {
            if (FCaptureState != CaptureState.Idle)
                return;

            FCaptureState = CaptureState.Writing;
            FSaveFilePath = filepath;

            DoReportProgress();

            foreach (var frame in FFrames)
                FGIF.AddFrame(frame.Image, FDelayInMilliseconds / 10);

            try
            {
                var fileStream = new FileStream(filepath, FileMode.OpenOrCreate);
                FGIF.Save(fileStream, paletteType, dithertype, colorCount);
                fileStream.Dispose();
            }
            catch
            {
                //silently fail
            }
            finally
            {
                FFrames.Clear();
                FGIF.Clear();

                FCaptureState = CaptureState.Idle;
                DoReportProgress();
            }
        }

        void DoReportProgress()
        {
            switch (FCaptureState)
            {
                case CaptureState.Idle:
                    FProgressBar.Invoke((MethodInvoker)delegate { FProgressBar.Hide(); });
                    return;
                case CaptureState.Capturing:
                    {
                        if (!FProgressBar.Visible)
                        {
                            RECT windowRect;
                            User32.GetWindowRect(FHandle, out windowRect);
                            FProgressLabel.BackColor = Color.LightGreen;

                            FProgressBar.Show();

                            var subX = FSubrect.HasValue ? FSubrect.Value.X : 0;
                            var subY = FSubrect.HasValue ? FSubrect.Value.Y + FSubrect.Value.Height : 0;
                            FProgressBar.SetDesktopLocation(windowRect.Left + subX, windowRect.Top + subY + 30);
                        }

                        FProgressLabel.Text = "Frame Count: " + FFrames.Count.ToString();
                        break;
                    }
                case CaptureState.Writing:
                    {
                        FProgressBar.Invoke((MethodInvoker)delegate { FProgressBar.Show(); });
                        FProgressLabel.Invoke((MethodInvoker)delegate
                        {
                            FProgressLabel.Text = FFrames.Count.ToString() + " frames. Writing File: " + FSaveFilePath;
                            FProgressLabel.BackColor = Color.LightSalmon;
                        });

                        break;
                    }
            }

            FProgressBar.Invoke((MethodInvoker)delegate { FProgressBar.ClientSize = new Size(FProgressLabel.Size.Width, FProgressLabel.Size.Height); });
        }
    }
}
