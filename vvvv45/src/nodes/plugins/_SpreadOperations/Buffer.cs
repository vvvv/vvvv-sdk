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

            if (FReset[0]) //set all slices to default
            {
                FOutput.SliceCount = frameCount;
                for (int i = 0; i < frameCount; i++)
                    FOutput[i] = FDefault[i].Clone() as ISpread<T>;
            }
      
            //set slice count
            if (FOutput.SliceCount > frameCount)
            {
                FOutput.RemoveRange(frameCount, FOutput.SliceCount - frameCount);
            }
            else if (FOutput.SliceCount < frameCount)
            {
                for (int i = FOutput.SliceCount; i < frameCount; i++)
                    FOutput.Add(new Spread<T>(1));
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
                    FOutput[bufferCounter] = FInput[i].Clone() as ISpread<T>;
                    FPhase[i] = frameCount > 1 ? bufferCounter / (double)(frameCount - 1) : 0;
                }  
            }

            FFirstFrame = false;
		}
	}
	
	
	[PluginInfo(Name = "Buffer",
	            Category = "Spreads",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ValueBufferNode : BufferNode<double>
	{
	}
	
	[PluginInfo(Name = "Buffer",
	            Category = "Color",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ColorBufferNode : BufferNode<RGBAColor>
	{
	}
	
	[PluginInfo(Name = "Buffer",
	            Category = "String",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class StringBufferNode : BufferNode<string>
	{
	}
	
	[PluginInfo(Name = "Buffer",
	            Category = "Transform",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class TransformBufferNode : BufferNode<Matrix4x4>
	{
	}
	
	[PluginInfo(Name = "Buffer",
	            Category = "Enumerations",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class EnumBufferNode : BufferNode<EnumEntry>
	{
	}
	
	[PluginInfo(Name = "Buffer",
	            Category = "Raw",
	            Help = "Inserts the input at the given index and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class EnumBufferNode : BufferNode<System.IO.Stream>
	{
	}
}