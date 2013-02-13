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
	public class QueueNode<T> : IPluginEvaluate
	{
		[Input("Input")]
		protected ISpread<T> FInput;
		
		[Input("Insert", IsSingle = true)]
		protected ISpread<bool> FDoInsert;
		
		[Input("Frame Count", IsSingle = true, MinValue = -1, DefaultValue = 1)]
		protected ISpread<int> FFrameCount;

        [Input("Reset", IsSingle = true, IsBang = true)]
        protected ISpread<bool> FReset;

		[Output("Output")]
		protected ISpread<ISpread<T>> FOutput;

		List<ISpread<T>> FBuffer = new List<ISpread<T>>();
		
		public void Evaluate(int SpreadMax)
		{
			//return null if one of the control inputs is null
            if(FDoInsert.IsAnyEmpty(FFrameCount, FReset))
            {
            	FOutput.SliceCount = 0;
            	return;
            }
			
            if (FReset[0])
                FBuffer.Clear();

        	if (FDoInsert[0])
        		FBuffer.Insert(0, FInput.Clone() as ISpread<T>);
			
        	var frameCount = FFrameCount[0];
        	if (frameCount >= 0 && FBuffer.Count > frameCount)
        		FBuffer.RemoveRange(frameCount, FBuffer.Count - frameCount);
			
			FOutput.AssignFrom(FBuffer);
		}
	}
	
	
	[PluginInfo(Name = "Queue",
	            Category = "Spreads",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ValueQueueNode : QueueNode<double>
	{
	}
	
	[PluginInfo(Name = "Queue",
	            Category = "Color",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ColorQueueNode : QueueNode<RGBAColor>
	{
	}
	
	[PluginInfo(Name = "Queue",
	            Category = "String",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class StringQueueNode : QueueNode<string>
	{
	}
	
	[PluginInfo(Name = "Queue",
	            Category = "Transform",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class TransformQueueNode : QueueNode<Matrix4x4>
	{
	}
	
	[PluginInfo(Name = "Queue",
	            Category = "Enumerations",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class EnumQueueNode : QueueNode<EnumEntry>
	{
	}
}