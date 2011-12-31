using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using System.Runtime.InteropServices;

namespace VVVV.MSKinect.Lib
{
    public enum eDepthMode { Disabled, DepthOnly, DepthAndPlayer }

    public class KinectRuntime
    {
        public Runtime Runtime { get; private set; }

        public bool IsStarted { get; private set; }

        public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady;
        public event EventHandler<ImageFrameReadyEventArgs> ColorFrameReady;
        public event EventHandler<ImageFrameReadyEventArgs> DepthFrameReady;

        public KinectStatus LastStatus { get; private set; }

        public eDepthMode DepthMode { get; private set; }


        public KinectRuntime()
        {
            
        }

        public void Assign(int idx)
        {
            if (this.Runtime != null)
            {
                this.Runtime.SkeletonFrameReady -= Runtime_SkeletonFrameReady;
                this.Runtime.VideoFrameReady -= Runtime_VideoFrameReady;
                this.Runtime.DepthFrameReady -= Runtime_DepthFrameReady;
                this.Runtime = null;
            }

            if (this.IsStarted)
            {
                this.Stop();
            }

            if (Runtime.Kinects.Count > 0)
            {
                this.Runtime = Runtime.Kinects[idx % Runtime.Kinects.Count];
                this.Runtime.SkeletonFrameReady += Runtime_SkeletonFrameReady;
                this.Runtime.VideoFrameReady += Runtime_VideoFrameReady;
                this.Runtime.DepthFrameReady += Runtime_DepthFrameReady;
            }
        }

        void Runtime_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            if (this.DepthFrameReady != null)
            {
                this.DepthFrameReady(sender, e);
            }             
        }

        void Runtime_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
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
                        RuntimeOptions options = (RuntimeOptions)0;
                        if (color) { options |= RuntimeOptions.UseColor; }
                        if (skeleton) { options |= RuntimeOptions.UseSkeletalTracking; }

                        if (depthmode == eDepthMode.DepthOnly) { options |= RuntimeOptions.UseDepth; }
                        if (depthmode == eDepthMode.DepthAndPlayer) { options |= RuntimeOptions.UseDepthAndPlayerIndex; }


                        this.Runtime.Initialize(options);
                        this.Runtime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);

                        if (depthmode != eDepthMode.Disabled)
                        {
                            ImageType it = ImageType.Depth;
                            if (depthmode == eDepthMode.DepthAndPlayer) { it = ImageType.DepthAndPlayerIndex; }
                            this.Runtime.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, it);
                        }

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
                        this.Runtime.Uninitialize();
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
