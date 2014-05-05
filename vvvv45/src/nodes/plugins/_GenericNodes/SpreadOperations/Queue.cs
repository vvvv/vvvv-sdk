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
using VVVV.Nodes.Generic;
#endregion usings

namespace VVVV.Nodes
{
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
	
	[PluginInfo(Name = "Queue",
	            Category = "Raw",
	            Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO fashion",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class RawQueueNode : QueueNode<System.IO.Stream>
	{
        protected override ISpread<System.IO.Stream> CloneInputSpread(ISpread<System.IO.Stream> spread)
        {
            var clone = new Spread<System.IO.Stream>(spread.SliceCount);
            for (int i = 0; i < clone.SliceCount; i++)
            {
                var stream = spread[i];
                var clonedStream = new System.IO.MemoryStream((int)stream.Length);
                stream.CopyTo(clonedStream);
                clone[i] = clonedStream;
            }
            return clone;
        }
	}
}