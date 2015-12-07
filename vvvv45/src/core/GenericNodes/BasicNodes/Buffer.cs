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

using VVVV.Nodes;
#endregion usings

namespace VVVV.Nodes.Generic
{
	public class BufferNode<T> : IPluginEvaluate
	{
		[Input("Input")]
		protected ISpread<ISpread<T>> FInput;

        [Input("Index")]
        protected ISpread<int> FIndex;
		
		[Input("Set")]
		protected ISpread<bool> FDoInsert;
		
		[Input("Frame Count", IsSingle = true, MinValue = 0, DefaultValue = 1)]
		protected ISpread<int> FFrameCount;

        [Input("Default")]
        protected ISpread<ISpread<T>> FDefault;

        [Input("Reset", IsSingle = true, IsBang = true)]
        protected ISpread<bool> FReset;

		[Output("Output")]
		protected ISpread<ISpread<T>> FOutput;

        [Output("Phase")]
        protected ISpread<double> FPhase;

        readonly Copier<T> FCopier;

        public BufferNode(Copier<T> copier)
        {
            FCopier = copier;
        }

        bool FFirstFrame = true;

		public void Evaluate(int SpreadMax)
		{
            //create new buffer on startup
            if (FFirstFrame) FOutput[0] = new Spread<T>(1);
            
            //return null if one of the control inputs is null
            if(FIndex.IsAnyEmpty(FDoInsert, FFrameCount, FReset))
            {
            	FOutput.SliceCount = 0;
            	FPhase.SliceCount = 0;
            	return;
            }

            //get buffer size
            var frameCount = FFrameCount[0];

            if (FReset[0] || FFirstFrame) //set all slices to default
            {
                FOutput.SliceCount = frameCount;
                for (int i = 0; i < frameCount; i++)
                    FOutput[i] = FCopier.CopySpread(FDefault[i]);
            }
      
            //set slice count
            if (FOutput.SliceCount > frameCount)
            {
                FOutput.RemoveRange(frameCount, FOutput.SliceCount - frameCount);
            }
            else if (FOutput.SliceCount < frameCount)
            {
                for (int i = FOutput.SliceCount; i < frameCount; i++)
                    FOutput.Add(FCopier.CopySpread(FDefault[i]));
            }

            SpreadMax = Math.Max(Math.Max(FInput.SliceCount, FIndex.SliceCount), FDoInsert.SliceCount);
            
            //set phase slice count
            FPhase.SliceCount = SpreadMax;

            //per slice
            for (int i = 0; i < SpreadMax; i++)
            {
                var bufferCounter = FIndex[i];
                bufferCounter %= Math.Max(frameCount, 1);

                if (FDoInsert[i])
                {
                    FOutput[bufferCounter] = FCopier.CopySpread(FInput[i]);
                    FPhase[i] = frameCount > 1 ? bufferCounter / (double)(frameCount - 1) : 0;
                }  
            }

            FFirstFrame = false;
		}
	}
}