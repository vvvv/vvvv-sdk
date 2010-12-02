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
		
		[Input("do Insert", IsSingle = true)]
		protected ISpread<bool> FDoInsert;
		
		[Input("Frame Count", IsSingle = true, MinValue = 0, DefaultValue = 1)]
		protected ISpread<int> FFrameCount;

		[Output("Output")]
		protected ISpread<ISpread<T>> FOutput;

		List<ISpread<T>> FBuffer = new List<ISpread<T>>();
		
		public void Evaluate(int SpreadMax)
		{
        	if (FDoInsert[0])
        		FBuffer.Insert(0, FInput.Clone());
			
        	var frameCount = FFrameCount[0];
        	if (FBuffer.Count > frameCount)
        		FBuffer.RemoveRange(frameCount, FBuffer.Count - frameCount);
			
			FOutput.AssignFrom(FBuffer);
		}
	}
	
	
	[PluginInfo(Name = "Queue",
	            Category = "Spreads",
	            Version = "",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ValueQueueNode : QueueNode<double>
	{
	}
	
	[PluginInfo(Name = "Queue",
	            Category = "Color",
	            Version = "",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class ColorQueueNode : QueueNode<RGBAColor>
	{
	}
	
	[PluginInfo(Name = "Queue",
	            Category = "String",
	            Version = "",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class StringQueueNode : QueueNode<string>
	{
	}
	
	[PluginInfo(Name = "Queue",
	            Category = "Transform",
	            Version = "",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class TransformQueueNode : QueueNode<Matrix4x4>
	{
	}
	
	[PluginInfo(Name = "Queue",
	            Category = "Enumerations",
	            Version = "",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class EnumQueueNode : QueueNode<EnumEntry>
	{
	}
}