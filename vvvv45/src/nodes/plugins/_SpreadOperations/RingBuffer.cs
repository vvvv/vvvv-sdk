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

        [Input("Insert", IsSingle = true)]
		protected ISpread<bool> FDoInsert;
		
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

        int FBufferCounter = 0;

        bool FFirstFrame = true;
		
		public void Evaluate(int SpreadMax)
		{
            //create new buffer on startup
            if (FFirstFrame) FOutput[0] = new Spread<T>(1);

            if (FReset[0])
            {
                FOutput.SliceCount = 0;
                FBufferCounter = 0;
            }

            var frameCount = FFrameCount[0];

            //remove slices
            if (FOutput.SliceCount > frameCount)
            {
                FOutput.RemoveRange(frameCount, FOutput.SliceCount - frameCount);
            }

            FBufferCounter %= Math.Max(frameCount, 1);

            if (FDoInsert[0])
            {
                //add slice, or put into place
                if (FOutput.SliceCount < frameCount && FBufferCounter >= FOutput.SliceCount)
                {
                    FOutput.Add(FInput.Clone());
                }
                else
                {
                    FOutput[FBufferCounter] = FInput.Clone();
                }

                FPhase[0] = frameCount > 0 ? FBufferCounter / (double)(frameCount-1) : 0;
                FCurrentFrame[0] = FBufferCounter;
                FBufferCounter++;
            }

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