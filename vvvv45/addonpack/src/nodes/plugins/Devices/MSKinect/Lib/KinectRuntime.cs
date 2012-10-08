using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Kinect;

namespace VVVV.MSKinect.Lib
{
    public enum eDepthMode { Disabled, DepthOnly, DepthAndPlayer }

    public class KinectRuntime
    {
        public KinectSensor Runtime { get; private set; }

        public bool IsStarted { get; private set; }

        public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady;
        public event EventHandler<ColorImageFrameReadyEventArgs> ColorFrameReady;
        public event EventHandler<DepthImageFrameReadyEventArgs> DepthFrameReady;

        public KinectStatus LastStatus { get; private set; }

        public eDepthMode DepthMode { get; private set; }


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

        public void SetDepthMode(eDepthMode mode)
        {
            if (mode != eDepthMode.Disabled)
            {
                this.Runtime.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
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
        }

        #region Start
        public void Start(bool color, bool skeleton, eDepthMode depthmode)
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

                this.DepthMode = depthmode;
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
