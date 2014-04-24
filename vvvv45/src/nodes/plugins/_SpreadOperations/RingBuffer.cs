#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.Hosting.Pins;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{

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
	
	[PluginInfo(Name = "RingBuffer",
	            Category = "Raw",
	            Help = "Inserts the input at the ringbuffer position and returns the whole Buffer as spread",
	            Tags = "",
	            AutoEvaluate = true
	           )]
	public class RawRingBufferNode : RingBufferNode<System.IO.Stream>
	{
	}
}