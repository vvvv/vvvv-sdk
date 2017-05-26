#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Hosting.Pins;
#endregion usings

namespace VVVV.Nodes.Generic
{
	public class RingBufferNode<T> : IPluginEvaluate
	{
		[Input("Input")]
		protected ISpread<T> FInput;

        [Input("Set", IsSingle = true)]
		protected ISpread<bool> FDoSet;
		
		[Input("Frame Count", IsSingle = true, MinValue = 0, DefaultValue = 1)]
		protected ISpread<int> FFrameCount;

        [Input("Reset", IsSingle = true, IsBang = true)]
        protected ISpread<bool> FReset;

		[Output("Output")]
		protected ISpread<ISpread<T>> FOutput;

        [Output("Phase", IsSingle = true)]
        protected ISpread<double> FPhase;

        [Output("Current Frame", IsSingle = true)]
        protected ISpread<int> FCurrentFrame;

        readonly Copier<T> FCopier;

        public RingBufferNode(Copier<T> copier)
        {
            FCopier = copier;
        }

        int FCurrentPos = -1;

        bool FFirstFrame = true;
		
		public void Evaluate(int SpreadMax)
		{
			//return null if one of the control inputs is null
            if(FDoSet.IsAnyEmpty(FFrameCount, FReset))
            {
            	FOutput.SliceCount = 0;
            	FPhase.SliceCount = 0;
            	FCurrentFrame.SliceCount = 0;
            	return;
            }
			
            if ((FFirstFrame) || (FReset[0]))
            {
                FOutput.SliceCount = 0;
                FCurrentPos = -1;
            }

            var frameCount = FFrameCount[0];
            var oldframecount = FOutput.SliceCount;

            FOutput.SliceCount = frameCount;
            FPhase.SliceCount = 1;
            FCurrentFrame.SliceCount = 1;

            if (oldframecount < frameCount)
                for (int i = oldframecount; i < frameCount; i++)
                    FOutput[i] = new Spread<T>(0);

            if (FDoSet[0])
            {
                FCurrentPos++;
                if (FCurrentPos > frameCount - 1)
                    FCurrentPos = 0;
                FOutput[FCurrentPos] = FCopier.CopySpread(FInput);
            }

            FCurrentFrame[0] = Math.Min(Math.Max(FCurrentPos, 0), frameCount - 1);
            FPhase[0] = frameCount > 1 ? FCurrentFrame[0] / (double)(frameCount - 1) : 1;

            FFirstFrame = false;
		}
	}
}