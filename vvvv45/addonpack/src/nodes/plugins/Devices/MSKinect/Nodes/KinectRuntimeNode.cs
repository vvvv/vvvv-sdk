using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.MSKinect.Lib;
using VVVV.Utils.VMath;
using Microsoft.Kinect;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "Kinect", 
	            Category = "Devices",
	            Version="Microsoft DX9", 
	            Author = "vux", 
	            Tags = "EX9",
	            Help = "Provides access to a Kinect through the MSKinect API")]
    public class KinectRuntimeNode : IPluginEvaluate, IDisposable
    {
        [Input("Motor Angle", IsSingle = true,DefaultValue=0.5)]
        IDiffSpread<double> FInAngle;

        [Input("Index", IsSingle = true)]
        IDiffSpread<int> FInIndex;

        [Input("Enable Color", IsSingle = true)]
        IDiffSpread<bool> FInEnableColor;

        [Input("Enable Depth", IsSingle = true)]
        IDiffSpread<bool> FInEnableDepth;

        [Input("Depth Range", IsSingle = true)]
        IDiffSpread<DepthRange> FInDepthRange;

        [Input("Enable Skeleton", IsSingle = true)]
        IDiffSpread<bool> FInEnableSkeleton;

        [Input("Enable Skeleton Smoothing", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FInEnableSmooth;

        [Input("Smooth Parameters", IsSingle = true)]
        Pin<TransformSmoothParameters> FSmoothParams;

        [Input("Enabled", IsSingle = true)]
        IDiffSpread<bool> FInEnabled;

        [Input("Reset", IsBang = true)]
        ISpread<bool> FInReset;

        [Output("Kinect Runtime",IsSingle=true)]
        ISpread<KinectRuntime> FOutRuntime;

        [Output("Kinect Count", IsSingle = true)]
        ISpread<int> FOutKCnt;

        [Output("Kinect Status", IsSingle = true)]
        ISpread<KinectStatus> FOutStatus;

        [Output("Is Started", IsSingle = true)]
        ISpread<bool> FOutStarted;

        [Output("Color FOV")]
        ISpread<Vector2D> FOutColorFOV;

        [Output("Depth FOV")]
        ISpread<Vector2D> FOutDepthFOV;

        private KinectRuntime runtime = new KinectRuntime();

        private bool haskinect = false;

        public void Evaluate(int SpreadMax)
        {
            
            bool reset = false;

            if (this.FInIndex.IsChanged || this.FInReset[0] || this.runtime.Runtime == null)
            {
                this.haskinect = this.runtime.Assign(this.FInIndex[0]);
                reset = true;   
            }

            if (this.haskinect)
            {

                if (this.FInEnabled.IsChanged || reset)
                {
                    if (this.FInEnabled[0])
                    {
                        this.runtime.Start(this.FInEnableColor[0], this.FInEnableSkeleton[0], this.FInEnableDepth[0]);
                    }
                    else
                    {
                        this.runtime.Stop();
                    }

                    reset = true;
                }

                if (this.FInEnableDepth.IsChanged || reset)
                {
                    this.runtime.SetDepthMode(this.FInEnableDepth[0]);
                }

                if (this.FInEnableColor.IsChanged || reset)
                {
                    this.runtime.SetColor(this.FInEnableColor[0]);
                }


                if (this.FInDepthRange.IsChanged || reset)
                {
                    try
                    {
                        this.runtime.SetDepthRange(this.FInDepthRange[0]);
                    }
                    catch { }
                }


                if (this.FInEnableSkeleton.IsChanged || this.FInEnableSmooth.IsChanged || this.FSmoothParams.IsChanged || reset)
                {
                    TransformSmoothParameters sp;
                    if (this.FSmoothParams.PluginIO.IsConnected)
                    {
                        sp = this.FSmoothParams[0];
                    }
                    else
                    {
                        sp = this.runtime.DefaultSmooth();
                    }

                    this.runtime.EnableSkeleton(this.FInEnableSkeleton[0],this.FInEnableSmooth[0],sp);
                }

                if (this.FInAngle.IsChanged || reset)
                {
                    if (this.runtime.IsStarted)
                    {
                        try { this.runtime.Runtime.ElevationAngle = (int)VMath.Map(this.FInAngle[0], 0, 1, this.runtime.Runtime.MinElevationAngle, this.runtime.Runtime.MaxElevationAngle, TMapMode.Clamp); }
                        catch { }
                    }
                }

                
                this.FOutStatus[0] = runtime.Runtime.Status;
                this.FOutRuntime[0] = runtime;
                this.FOutStarted[0] = runtime.IsStarted;

                this.FOutColorFOV.SliceCount = 1;
                this.FOutDepthFOV.SliceCount = 1;

                this.FOutColorFOV[0] = new Vector2D(this.runtime.Runtime.ColorStream.NominalHorizontalFieldOfView,
                                                    this.runtime.Runtime.ColorStream.NominalVerticalFieldOfView) * (float)VMath.DegToCyc;

                this.FOutDepthFOV[0] = new Vector2D(this.runtime.Runtime.DepthStream.NominalHorizontalFieldOfView ,
                    								this.runtime.Runtime.DepthStream.NominalVerticalFieldOfView) * (float)VMath.DegToCyc;
            }

            this.FOutKCnt[0] = KinectSensor.KinectSensors.Count;
        }

        public void Dispose()
        {
            if (this.runtime != null)
            {
                this.runtime.Stop();
                if (this.runtime.Runtime != null)
                {
                    this.runtime.Runtime.Dispose();
                }
            }
        }
    }
}
