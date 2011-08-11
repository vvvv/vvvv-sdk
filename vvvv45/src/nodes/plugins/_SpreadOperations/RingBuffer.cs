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

namespace VVVV.Nodes
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

        int FCurrentPos = 0;

        bool FFirstFrame = true;
		
		public void Evaluate(int SpreadMax)
		{
            if ((FFirstFrame) || (FReset[0])) FOutput.SliceCount = 0;

            var frameCount = FFrameCount[0];
            var oldframecount = FOutput.SliceCount;

            FOutput.SliceCount = frameCount;

            if (oldframecount < frameCount)
                for (int i = oldframecount; i < frameCount; i++)
                    FOutput[i] = new Spread<T>(0);

            if (FDoSet[0])
            {
                FCurrentPos++;
                FCurrentPos %= Math.Max(frameCount, 1);

                FOutput[FCurrentPos] = FInput.Clone();
                FCurrentFrame[0] = FCurrentPos;
            }

            FPhase[0] = frameCount > 0 ? FCurrentPos / (double)(frameCount - 1) : 0;
            FFirstFrame = false;
		}
	}
	
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "Spreads",
	            Help = "Inserts the input at the ringbuffer position and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ValueRingBufferNode : RingBufferNode<double>
	{
	}
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "Color",
	            Help = "Inserts the input at the ringbuffer position and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ColorRingBufferNode : RingBufferNode<RGBAColor>
	{
	}
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "String",
	            Help = "Inserts the input at the ringbuffer position and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class StringRingBufferNode : RingBufferNode<string>
	{
	}
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "Transform",
	            Help = "Inserts the input at the ringbuffer position and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class TransformRingBufferNode : RingBufferNode<Matrix4x4>
	{
	}
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "Enumerations",
	            Help = "Inserts the input at the ringbuffer position and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class EnumRingBufferNode : RingBufferNode<EnumEntry>
	{
	}
}