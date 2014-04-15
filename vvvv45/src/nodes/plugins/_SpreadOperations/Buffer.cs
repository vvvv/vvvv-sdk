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
	public class RawBufferNode : BufferNode<System.IO.Stream>
	{
	}
}