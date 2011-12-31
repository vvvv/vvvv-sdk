using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using Microsoft.Research.Kinect.Nui;
using VVVV.MSKinect.Lib;
using VVVV.Utils.VMath;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "Kinect", Category = "Devices",Version="Microsoft", Author = "vux")]
    public class KinectRuntimeNode : IPluginEvaluate
    {
        [Input("Motor Angle")]
        IDiffSpread<double> FInAngle;

        [Input("Index")]
        IDiffSpread<int> FInIndex;

        [Input("Enable Color")]
        IDiffSpread<bool> FInEnableColor;

        [Input("Depth Mode")]
        IDiffSpread<eDepthMode> FInDepthMode;
        
        [Input("Enable Skeleton")]
        IDiffSpread<bool> FInEnableSkeleton;

        [Input("Enabled")]
        IDiffSpread<bool> FInEnabled;

        [Input("Reset",IsBang = true)]
        ISpread<bool> FInReset;

        [Output("Kinect Runtime",IsSingle=true)]
        ISpread<KinectRuntime> FOutRuntime;

        [Output("Kinect Count", IsSingle = true)]
        ISpread<int> FOutKCnt;

        [Output("Kinect Status", IsSingle = true)]
        ISpread<KinectStatus> FOutStatus;

        [Output("Is Started", IsSingle = true)]
        ISpread<bool> FOutStarted;

        private KinectRuntime runtime = new KinectRuntime();

        public void Evaluate(int SpreadMax)
        {
            bool reset = false;

            if (this.FInIndex.IsChanged || this.FInReset[0])
            {
                this.runtime.Assign(this.FInIndex[0]);
                reset = true;
            }


            if (this.FInEnableColor.IsChanged
                || this.FInEnabled.IsChanged
                || this.FInDepthMode.IsChanged
                || this.FInEnableSkeleton.IsChanged
                || reset)
            {
                if (this.FInEnabled[0])
                {
                    this.runtime.Start(this.FInEnableColor[0], this.FInEnableSkeleton[0],this.FInDepthMode[0]);
                }
                else
                {
                    this.runtime.Stop();
                }

                reset = true;
            }

            if (this.FInAngle.IsChanged || reset)
            {
                if (this.runtime.IsStarted)
                {
                    try { this.runtime.Runtime.NuiCamera.ElevationAngle = (int)VMath.Map(this.FInAngle[0], 0, 1, Camera.ElevationMinimum, Camera.ElevationMaximum, TMapMode.Clamp); }
                    catch { }
                }
            }

            this.FOutKCnt[0] = Runtime.Kinects.Count;
            //this.FOutStatus[0] = runtime.Runtime.Status;
            this.FOutRuntime[0] = runtime;
            this.FOutStarted[0] = runtime.IsStarted;
        }
    }
}
