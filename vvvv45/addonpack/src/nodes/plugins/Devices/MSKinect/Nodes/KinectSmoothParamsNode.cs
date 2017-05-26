using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using Microsoft.Kinect;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace MSKinect.Nodes
{
    [PluginInfo(Name = "SmoothSettings", 
	            Category = "Kinect", 
	            Version = "Microsoft DX9", 
	            Author = "vux", 
	            Tags = "EX9",
	            Help = "Sets smoothing parameters for skeleton tracking")]
    public class KinectSmoothParamsNode : IPluginEvaluate
    {
        [Input("Correction", IsSingle = true, DefaultValue = 0.5)]
        IDiffSpread<float> FInCorrection;

        [Input("Jitter Radius", IsSingle = true, DefaultValue = 0.05)]
        IDiffSpread<float> FInJitter;

        [Input("Max Deviation Radius", IsSingle = true, DefaultValue = 0.04)]
        IDiffSpread<float> FInDevRadius;

        [Input("Prediction", IsSingle = true, DefaultValue = 0.5)]
        IDiffSpread<float> FInPrediction;

        [Input("Smoothing", IsSingle = true, DefaultValue = 0.5)]
        IDiffSpread<float> FInSmoothing;

        [Output("Output", IsSingle = true)]
        ISpread<TransformSmoothParameters> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (AnySpreadChanged(FInCorrection, FInJitter, FInDevRadius, FInPrediction, FInSmoothing))
            {
                TransformSmoothParameters sp = new TransformSmoothParameters();
                sp.Correction = FInCorrection[0];
                sp.JitterRadius = FInJitter[0];
                sp.MaxDeviationRadius = FInDevRadius[0];
                sp.Prediction = FInPrediction[0];
                sp.Smoothing = FInSmoothing[0];
                FOut[0] = sp;
            }
        }

        private bool AnySpreadChanged(params IDiffSpread[] pins)
        {
            foreach (IDiffSpread ds in pins)
            {
                if (ds.IsChanged) { return true; }
            }

            return false;
        }
    }
}
