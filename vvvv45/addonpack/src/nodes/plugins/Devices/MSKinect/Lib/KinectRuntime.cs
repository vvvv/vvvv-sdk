using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Kinect;

namespace VVVV.MSKinect.Lib
{
    public class KinectRuntime
    {
        public KinectSensor Runtime { get; private set; }

        public bool IsStarted { get; private set; }

        public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady;
        public event EventHandler<ColorImageFrameReadyEventArgs> ColorFrameReady;
        public event EventHandler<DepthImageFrameReadyEventArgs> DepthFrameReady;
        public event EventHandler<AllFramesReadyEventArgs> AllFrameReady;

        public KinectStatus LastStatus { get; private set; }

        public static readonly int[] FACE_INDICES = new int[] { 11, 0, 1, 1, 0, 34, 34, 0, 44, 12, 11, 1, 34, 44, 45, 12, 1, 13, 13, 1, 2, 2, 1, 34, 46, 2, 34, 46, 34, 45, 14, 12, 13, 14, 13, 15, 15, 13, 16, 13, 2, 16, 16, 2, 17, 17, 2, 3, 3, 2, 50, 50, 2, 49, 49, 2, 46, 49, 46, 48, 48, 46, 47, 46, 45, 47, 29, 14, 15, 29, 15, 20, 15, 18, 19, 16, 18, 15, 18, 16, 17, 18, 17, 19, 23, 17, 77, 17, 3, 77, 78, 3, 50, 56, 78, 50, 52, 50, 51, 50, 49, 51, 49, 48, 51, 51, 48, 52, 53, 48, 62, 48, 47, 62, 28, 29, 27, 29, 20, 27, 26, 24, 25, 58, 57, 59, 60, 53, 62, 60, 62, 61, 26, 111, 33, 26, 75, 111, 25, 75, 26, 59, 76, 58, 112, 76, 59, 66, 112, 59, 33, 6, 7, 7, 6, 66, 32, 9, 10, 10, 9, 65, 76, 6, 5, 5, 6, 75, 77, 3, 78, 33, 7, 79, 79, 7, 81, 81, 7, 87, 80, 7, 66, 82, 7, 80, 87, 7, 82, 82, 80, 89, 89, 80, 64, 88, 79, 81, 31, 79, 88, 79, 26, 33, 31, 26, 79, 66, 59, 80, 80, 59, 64, 88, 83, 85, 88, 85, 31, 83, 8, 85, 83, 40, 8, 8, 40, 84, 8, 84, 86, 89, 86, 84, 64, 86, 89, 9, 85, 8, 9, 8, 86, 85, 32, 31, 9, 32, 85, 65, 86, 64, 65, 9, 86, 26, 27, 24, 30, 90, 32, 32, 90, 31, 90, 30, 28, 90, 26, 31, 90, 27, 26, 90, 28, 27, 60, 59, 57, 65, 91, 63, 64, 91, 65, 91, 61, 63, 91, 64, 59, 91, 59, 60, 91, 60, 61, 77, 92, 23, 23, 92, 25, 25, 92, 75, 56, 93, 78, 76, 93, 58, 58, 93, 56, 77, 94, 92, 92, 94, 75, 75, 94, 5, 5, 94, 76, 76, 94, 93, 93, 94, 78, 78, 94, 77, 95, 20, 15, 97, 20, 95, 101, 20, 99, 27, 20, 101, 19, 95, 15, 21, 95, 19, 97, 95, 21, 27, 101, 24, 24, 101, 22, 22, 101, 99, 23, 103, 17, 23, 105, 103, 23, 109, 107, 23, 25, 109, 17, 103, 19, 19, 103, 21, 21, 103, 105, 107, 109, 22, 22, 109, 24, 24, 109, 25, 104, 56, 50, 106, 56, 104, 110, 56, 108, 58, 56, 110, 52, 104, 50, 54, 104, 52, 106, 104, 54, 55, 110, 108, 57, 110, 55, 58, 110, 57, 48, 53, 96, 53, 98, 96, 100, 53, 102, 102, 53, 60, 48, 96, 52, 52, 96, 54, 54, 96, 98, 100, 102, 55, 55, 102, 57, 57, 102, 60, 6, 111, 75, 33, 111, 6, 76, 112, 6, 6, 112, 66, 74, 73, 70, 70, 73, 69, 68, 67, 72, 72, 67, 71, 69, 53, 70, 74, 56, 73, 71, 23, 72, 68, 20, 67, 69, 98, 53, 69, 54, 98, 73, 54, 69, 106, 54, 73, 56, 106, 73, 108, 56, 74, 74, 55, 108, 70, 55, 74, 100, 55, 70, 53, 100, 70, 67, 20, 97, 67, 97, 21, 67, 21, 71, 71, 21, 105, 71, 105, 23, 99, 20, 68, 99, 68, 22, 72, 22, 68, 107, 22, 72, 23, 107, 72, 63, 61, 113, 61, 62, 113, 113, 62, 114, 62, 47, 114, 114, 47, 115, 47, 45, 115, 113, 114, 116, 114, 115, 116, 28, 30, 117, 29, 28, 117, 29, 117, 118, 14, 29, 118, 14, 118, 119, 12, 14, 119, 118, 117, 120, 119, 118, 120, 88, 81, 83, 81, 40, 83, 82, 89, 84, 87, 82, 84, 81, 87, 40, 87, 84, 40 };

        public KinectRuntime()
        {
            
        }

        public bool Assign(int idx)
        {
            if (this.Runtime != null)
            {
                
                this.Runtime.SkeletonFrameReady -= Runtime_SkeletonFrameReady;
                this.Runtime.ColorFrameReady -= Runtime_ColorFrameReady;
                this.Runtime.DepthFrameReady -= Runtime_DepthFrameReady;
                this.Runtime.AllFramesReady -= Runtime_AllFramesReady;
                this.Runtime = null;
            }

            if (this.IsStarted)
            {
                this.Stop();
            }

            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.Runtime = KinectSensor.KinectSensors[idx % KinectSensor.KinectSensors.Count];
                return true;
            }
            else
            {
                return false;
            }
        }

        void Runtime_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (this.AllFrameReady != null)
            {
                this.AllFrameReady(sender, e);
            }
        }

        void Runtime_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (this.DepthFrameReady != null)
            {
                this.DepthFrameReady(sender, e);
            }             
        }

        private void Runtime_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            if (this.ColorFrameReady != null)
            {
                this.ColorFrameReady(sender, e);
            }           
        }

        private void Runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (this.SkeletonFrameReady != null)
            {
                this.SkeletonFrameReady(sender, e);
            }
        }

        public void EnableSkeleton(bool enable, bool smooth, TransformSmoothParameters sp)
        {
            if (enable)
            {
                //Need to disable
                if (this.Runtime.SkeletonStream.IsEnabled)
                {
                    this.Runtime.SkeletonStream.Disable();
                }

                if (smooth)
                {
                    this.Runtime.SkeletonStream.Enable(sp);
                }
                else
                {
                    this.Runtime.SkeletonStream.Enable();
                }


                this.Runtime.SkeletonFrameReady += this.Runtime_SkeletonFrameReady;
            }
            else
            {
                this.Runtime.SkeletonStream.Disable();
                this.Runtime.SkeletonFrameReady -= this.Runtime_SkeletonFrameReady;
            }
        }

        public TransformSmoothParameters DefaultSmooth()
        {
            TransformSmoothParameters sp = new TransformSmoothParameters();
            sp.Correction = 0.5f;
            sp.JitterRadius = 0.05f;
            sp.MaxDeviationRadius = 0.04f;
            sp.Prediction = 0.5f;
            sp.Smoothing = 0.5f;

            return sp;
        }

        public void SetDepthMode(bool enable)
        {
            if (enable)
            {
                this.Runtime.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.Runtime.DepthFrameReady += this.Runtime_DepthFrameReady;
            }
            else
            {
                this.Runtime.DepthFrameReady -= Runtime_DepthFrameReady;
                this.Runtime.DepthStream.Disable();
            }
        }

        public void SetColor(bool enable)
        {
            if (enable)
            {
                this.Runtime.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.Runtime.ColorFrameReady += this.Runtime_ColorFrameReady;
            }
            else
            {
                this.Runtime.ColorFrameReady -= Runtime_ColorFrameReady;
                this.Runtime.ColorStream.Disable();
            }

        }

        public void SetDepthRange(DepthRange range)
        {
            this.Runtime.DepthStream.Range = range;

            if (range == DepthRange.Near)
            {
                this.Runtime.SkeletonStream.EnableTrackingInNearRange = true;
            }
            else {
                this.Runtime.SkeletonStream.EnableTrackingInNearRange = false;
            }
        }

        public void SetSkeletonMode(SkeletonTrackingMode mode)
        {
            this.Runtime.SkeletonStream.TrackingMode = mode;
        }

        #region Start
        public void Start(bool color, bool skeleton, bool depth)
        {
            if (this.Runtime != null)
            {
                if (this.IsStarted)
                {
                    this.Stop();
                }

                if (!this.IsStarted)
                {
                    try
                    {

                        this.Runtime.Start();


                        this.IsStarted = true;
                    }
                    catch
                    {
                        this.IsStarted = false;
                    }
                }

                this.Runtime.AllFramesReady += Runtime_AllFramesReady;
            }
        }

        #endregion

        #region Stop
        public void Stop()
        {
            if (this.Runtime != null)
            {
                if (this.IsStarted)
                {
                    try
                    {
                        this.Runtime.SkeletonFrameReady -= Runtime_SkeletonFrameReady;
                        this.Runtime.ColorFrameReady -= Runtime_ColorFrameReady;
                        this.Runtime.DepthFrameReady -= Runtime_DepthFrameReady;
                        this.Runtime.Stop();
                        
                    }
                    catch
                    {

                    }
                }

                this.IsStarted = false;
            }
        }
        #endregion
    }
}
